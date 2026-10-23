using UrlShortener.Application.Abstractions.Mediator;
using UrlShortener.Application.Common.Results;

namespace UrlShortener.Application.Features.ShortUrls.RedirectShortUrl;

public sealed record RedirectShortUrlQuery(string ShortCode, ClickMetadata ClickMetadata)
    : IRequest<Result<RedirectShortUrlResponse>>;
