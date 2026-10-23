using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Abstractions.Persistence;
using UrlShortener.Application.Features.ShortUrls.GetLinkStats;

namespace UrlShortener.Infrastructure.Persistence.Repositories;

public sealed class ClickRepository(AppDbContext db) : IClickRepository
{
    private const int TopListSize = 10;

    public async Task<ClickStatsSnapshot> GetStatsAsync(Guid shortUrlId, string timeZoneId, CancellationToken ct)
    {
        var clicks = db.ClickEvents.AsNoTracking().Where(c => c.ShortUrlId == shortUrlId);

        var total = await clicks.CountAsync(ct);

        // Npgsql translates ConvertTimeBySystemTimeZoneId to AT TIME ZONE, so days are grouped in the database
        var daily = await clicks
            .GroupBy(c => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(c.OccurredAtUtc, timeZoneId).Date)
            .Select(g => new { Date = g.Key, Clicks = g.Count() })
            .ToListAsync(ct);

        var countries = await clicks
            .GroupBy(c => c.CountryCode)
            .OrderByDescending(g => g.Count())
            .Take(TopListSize)
            .Select(g => new CountryClicks(g.Key, g.Count()))
            .ToListAsync(ct);

        var referrers = await clicks
            .GroupBy(c => c.RefererHost)
            .OrderByDescending(g => g.Count())
            .Take(TopListSize)
            .Select(g => new ReferrerClicks(g.Key, g.Count()))
            .ToListAsync(ct);

        return new ClickStatsSnapshot(
            total,
            daily.Select(d => new DailyClickCount(DateOnly.FromDateTime(d.Date), d.Clicks)).ToList(),
            countries,
            referrers);
    }
}
