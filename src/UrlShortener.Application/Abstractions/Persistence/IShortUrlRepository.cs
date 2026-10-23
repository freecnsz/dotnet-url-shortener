using UrlShortener.Application.Features.ShortUrls.RedirectShortUrl;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Abstractions.Persistence;

public interface IShortUrlRepository
{
    void Add(ShortUrl shortUrl);
    Task<ShortUrl?> GetByShortCodeAsync(string shortCode, CancellationToken ct);
    Task<ResolvedUrl?> GetResolvedAsync(string shortCode, CancellationToken ct);
}
