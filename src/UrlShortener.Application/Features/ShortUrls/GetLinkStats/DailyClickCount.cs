namespace UrlShortener.Application.Features.ShortUrls.GetLinkStats;

public sealed record DailyClickCount(DateOnly Date, int Clicks);
