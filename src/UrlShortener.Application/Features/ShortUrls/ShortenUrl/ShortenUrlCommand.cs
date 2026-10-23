using UrlShortener.Application.Abstractions.Mediator;
using UrlShortener.Application.Common.Results;

namespace UrlShortener.Application.Features.ShortUrls.ShortenUrl;

public sealed record ShortenUrlCommand(string? OriginalUrl)
    : IRequest<Result<ShortenUrlResponse>>;
