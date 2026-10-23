using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using UrlShortener.Application.Abstractions.Persistence;
using UrlShortener.Application.Abstractions.Services;
using UrlShortener.Application.Features.ShortUrls.RedirectShortUrl;
using UrlShortener.Infrastructure.Options;

namespace UrlShortener.Infrastructure.Services;

public sealed class UrlLookup(
    IConnectionMultiplexer redis,
    IShortUrlRepository repository,
    IOptions<UrlCacheOptions> options,
    ILogger<UrlLookup> logger) : IUrlLookup
{
    private readonly UrlCacheOptions _options = options.Value;
    private IDatabase Cache => redis.GetDatabase();

    // Bump the version whenever the cached shape of ResolvedUrl changes
    private static string CacheKey(string shortCode) => $"url:v3:{shortCode}";

    public async Task<ResolvedUrl?> ResolveAsync(string shortCode, CancellationToken ct)
    {
        var cached = await TryReadCacheAsync(shortCode);
        if (cached is not null)
            return cached;

        var resolved = await repository.GetResolvedAsync(shortCode, ct);
        if (resolved is null)
            return null;

        await TryWriteCacheAsync(shortCode, resolved);
        return resolved;
    }

    public async Task InvalidateAsync(string shortCode)
    {
        try
        {
            await Cache.KeyDeleteAsync(CacheKey(shortCode));
        }
        catch (Exception ex) when (ex is RedisException or RedisTimeoutException)
        {
            // The entry expires on its own within the cache TTL
            logger.LogWarning(ex, "Cache invalidation failed. ShortCode: {ShortCode}", shortCode);
        }
    }

    // The cache is an optimization, not a dependency: any failure falls back to the database
    private async Task<ResolvedUrl?> TryReadCacheAsync(string shortCode)
    {
        try
        {
            var cached = await Cache.StringGetAsync(CacheKey(shortCode));
            return cached.HasValue
                ? JsonSerializer.Deserialize<ResolvedUrl>(cached.ToString())
                : null;
        }
        catch (Exception ex) when (ex is RedisException or RedisTimeoutException or JsonException)
        {
            logger.LogWarning(ex, "Cache read failed. ShortCode: {ShortCode}", shortCode);
            return null;
        }
    }

    private async Task TryWriteCacheAsync(string shortCode, ResolvedUrl resolved)
    {
        try
        {
            await Cache.StringSetAsync(CacheKey(shortCode), JsonSerializer.Serialize(resolved), _options.Ttl);
        }
        catch (Exception ex) when (ex is RedisException or RedisTimeoutException)
        {
            logger.LogWarning(ex, "Cache write failed. ShortCode: {ShortCode}", shortCode);
        }
    }
}
