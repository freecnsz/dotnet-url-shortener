using UrlShortener.Application.Features.ShortUrls.GetLinkStats;

namespace UrlShortener.Application.Abstractions.Persistence;

public interface IClickRepository
{
    Task<ClickStatsSnapshot> GetStatsAsync(Guid shortUrlId, string timeZoneId, CancellationToken ct);
}
