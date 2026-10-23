using UrlShortener.Application.Abstractions.Mediator;
using UrlShortener.Application.Common.Results;

namespace UrlShortener.Application.Features.ShortUrls.GetLinkStats;

public sealed record GetLinkStatsQuery(string ShortCode, string? ManageToken, string? TimeZone)
    : IRequest<Result<LinkStatsResponse>>;
