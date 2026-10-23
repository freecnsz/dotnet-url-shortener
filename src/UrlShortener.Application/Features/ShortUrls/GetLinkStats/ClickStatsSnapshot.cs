namespace UrlShortener.Application.Features.ShortUrls.GetLinkStats;

public sealed record ClickStatsSnapshot(
    int TotalClicks,
    IReadOnlyList<DailyClickCount> Daily,
    IReadOnlyList<CountryClicks> Countries,
    IReadOnlyList<ReferrerClicks> Referrers);
