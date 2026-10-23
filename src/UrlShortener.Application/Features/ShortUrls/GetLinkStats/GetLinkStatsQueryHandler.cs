using Microsoft.Extensions.Logging;
using UrlShortener.Application.Abstractions.Mediator;
using UrlShortener.Application.Abstractions.Persistence;
using UrlShortener.Application.Abstractions.Services;
using UrlShortener.Application.Common.Results;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Features.ShortUrls.GetLinkStats;

public sealed class GetLinkStatsQueryHandler(
    IShortUrlRepository shortUrlRepository,
    IManageTokenService manageTokenService,
    IClickRepository clickRepository,
    IAbsoluteUrlBuilder absoluteUrlBuilder,
    TimeProvider timeProvider,
    ILogger<GetLinkStatsQueryHandler> logger)
    : IRequestHandler<GetLinkStatsQuery, Result<LinkStatsResponse>>
{
    private const string DefaultTimeZone = "UTC";

    public async Task<Result<LinkStatsResponse>> Handle(GetLinkStatsQuery query, CancellationToken ct)
    {
        var timeZoneId = string.IsNullOrWhiteSpace(query.TimeZone) ? DefaultTimeZone : query.TimeZone;
        if (!TryFindIanaTimeZone(timeZoneId, out var timeZone))
            return ShortUrlErrors.InvalidTimeZone;

        if (!ShortUrl.IsValidShortCode(query.ShortCode))
            return ShortUrlErrors.NotFound;

        var shortUrl = await shortUrlRepository.GetByShortCodeAsync(query.ShortCode, ct);
        if (shortUrl is null || shortUrl.DeletedAtUtc is not null)
            return ShortUrlErrors.NotFound;

        if (!manageTokenService.Verify(query.ManageToken, shortUrl.ManageTokenHash))
        {
            logger.LogInformation("Manage token rejected. ShortCode: {ShortCode}", query.ShortCode);
            return ShortUrlErrors.InvalidManageToken;
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var stats = await clickRepository.GetStatsAsync(shortUrl.Id, timeZoneId, ct);
        var lastDayUtc = shortUrl.IsExpired(nowUtc) ? shortUrl.ExpiresAtUtc : nowUtc;

        var daily = FillMissingDays(
            stats.Daily,
            ToLocalDate(shortUrl.CreatedAtUtc, timeZone),
            ToLocalDate(lastDayUtc, timeZone));

        return new LinkStatsResponse(
            absoluteUrlBuilder.BuildAbsoluteUrl(shortUrl.ShortCode),
            shortUrl.OriginalUrl.Value,
            shortUrl.CreatedAtUtc,
            shortUrl.ExpiresAtUtc,
            shortUrl.IsExpired(nowUtc),
            timeZoneId,
            stats.TotalClicks,
            daily,
            stats.Countries,
            stats.Referrers);
    }

    // The id is passed to PostgreSQL, so it must be an IANA id (not a Windows id)
    private static bool TryFindIanaTimeZone(string id, out TimeZoneInfo timeZone)
    {
        if (id == DefaultTimeZone)
        {
            timeZone = TimeZoneInfo.Utc;
            return true;
        }

        return TimeZoneInfo.TryFindSystemTimeZoneById(id, out timeZone!) && timeZone.HasIanaId;
    }

    private static DateOnly ToLocalDate(DateTime utc, TimeZoneInfo timeZone)
        => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone));

    private static List<DailyClickCount> FillMissingDays(
        IReadOnlyList<DailyClickCount> sparse, DateOnly from, DateOnly to)
    {
        var counts = sparse.ToDictionary(d => d.Date, d => d.Clicks);
        var result = new List<DailyClickCount>();

        for (var day = from; day <= to; day = day.AddDays(1))
            result.Add(new DailyClickCount(day, counts.GetValueOrDefault(day)));

        return result;
    }
}
