using Microsoft.Extensions.Logging;
using UrlShortener.Application.Abstractions.Mediator;
using UrlShortener.Application.Abstractions.Services;
using UrlShortener.Application.Common.Results;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Features.ShortUrls.RedirectShortUrl;

public sealed class RedirectShortUrlQueryHandler(
    IUrlLookup urlLookup,
    IClickRecorder clickRecorder,
    TimeProvider timeProvider,
    ILogger<RedirectShortUrlQueryHandler> logger)
    : IRequestHandler<RedirectShortUrlQuery, Result<RedirectShortUrlResponse>>
{
    public async Task<Result<RedirectShortUrlResponse>> Handle(
        RedirectShortUrlQuery query, CancellationToken ct)
    {
        // A malformed code can never exist; skip cache and database entirely
        if (!ShortUrl.IsValidShortCode(query.ShortCode))
            return ShortUrlErrors.NotFound;

        var resolved = await urlLookup.ResolveAsync(query.ShortCode, ct);
        if (resolved is null)
        {
            logger.LogInformation("Short code not found. ShortCode: {ShortCode}", query.ShortCode);
            return ShortUrlErrors.NotFound;
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        if (nowUtc >= resolved.ExpiresAtUtc)
        {
            logger.LogInformation("Short code expired. ShortCode: {ShortCode}", query.ShortCode);
            return ShortUrlErrors.Expired;
        }

        RecordClick(resolved.ShortUrlId, nowUtc, query.ClickMetadata);

        logger.LogDebug("Redirecting. ShortCode: {ShortCode}", query.ShortCode);
        return new RedirectShortUrlResponse(resolved.OriginalUrl);
    }

    private void RecordClick(Guid shortUrlId, DateTime nowUtc, ClickMetadata metadata)
    {
        // Tracking is best-effort: a failure here must never break the redirect
        try
        {
            var click = ClickEvent.Create(
                shortUrlId, nowUtc, metadata.CountryCode, metadata.RefererHost, metadata.UserAgent);

            clickRecorder.Record(click);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to record click. ShortUrlId: {ShortUrlId}", shortUrlId);
        }
    }
}
