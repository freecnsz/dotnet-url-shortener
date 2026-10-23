namespace UrlShortener.Application.Features.ShortUrls.GetLinkStats;

public sealed record ReferrerClicks(string? RefererHost, int Clicks);
