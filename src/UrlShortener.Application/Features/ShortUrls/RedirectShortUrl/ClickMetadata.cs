namespace UrlShortener.Application.Features.ShortUrls.RedirectShortUrl;

public sealed record ClickMetadata(string? RefererHost, string? UserAgent, string? CountryCode);
