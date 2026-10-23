namespace UrlShortener.Application.Features.ShortUrls.GetLinkStats;

public sealed record LinkStatsResponse(
    string ShortUrl,
    string OriginalUrl,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsExpired,
    string TimeZone,
    int TotalClicks,
    IReadOnlyList<DailyClickCount> Daily,
    IReadOnlyList<CountryClicks> Countries,
    IReadOnlyList<ReferrerClicks> Referrers);
