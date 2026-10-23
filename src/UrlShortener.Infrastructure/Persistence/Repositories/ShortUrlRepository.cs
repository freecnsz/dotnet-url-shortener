using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Abstractions.Persistence;
using UrlShortener.Application.Features.ShortUrls.RedirectShortUrl;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Persistence.Repositories;

public sealed class ShortUrlRepository(AppDbContext db) : IShortUrlRepository
{
    public void Add(ShortUrl shortUrl)
        => db.ShortUrls.Add(shortUrl);

    public Task<ShortUrl?> GetByShortCodeAsync(string shortCode, CancellationToken ct)
        => db.ShortUrls.FirstOrDefaultAsync(s => s.ShortCode == shortCode, ct);

    public Task<ResolvedUrl?> GetResolvedAsync(string shortCode, CancellationToken ct)
        => db.ShortUrls
            .AsNoTracking()
            .Where(s => s.ShortCode == shortCode && s.DeletedAtUtc == null)
            .Select(s => new ResolvedUrl(s.Id, s.OriginalUrl.Value, s.ExpiresAtUtc))
            .FirstOrDefaultAsync(ct);
}
