using UrlShortener.Application.Features.ShortUrls.RedirectShortUrl;

namespace UrlShortener.Application.Abstractions.Services;

public interface IUrlLookup
{
    Task<ResolvedUrl?> ResolveAsync(string shortCode, CancellationToken ct);
    Task InvalidateAsync(string shortCode);
}
