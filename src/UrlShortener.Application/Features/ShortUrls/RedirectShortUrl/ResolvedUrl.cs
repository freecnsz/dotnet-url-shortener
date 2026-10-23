namespace UrlShortener.Application.Features.ShortUrls.RedirectShortUrl;

public sealed record ResolvedUrl(Guid ShortUrlId, string OriginalUrl, DateTime ExpiresAtUtc);
