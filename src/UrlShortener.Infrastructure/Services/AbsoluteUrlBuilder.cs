using Microsoft.Extensions.Options;
using UrlShortener.Application.Abstractions.Services;
using UrlShortener.Domain.ValueObjects;
using UrlShortener.Infrastructure.Options;

namespace UrlShortener.Infrastructure.Services;

public sealed class AbsoluteUrlBuilder(IOptions<ShortUrlOptions> options) : IAbsoluteUrlBuilder
{
    private readonly string _baseUrl = options.Value.BaseUrl.TrimEnd('/');
    private readonly string _baseHost = new Uri(options.Value.BaseUrl).Host;

    public string BuildAbsoluteUrl(string shortCode) => $"{_baseUrl}/{shortCode}";

    public bool PointsToSelf(OriginalUrl url)
        => string.Equals(new Uri(url.Value).Host, _baseHost, StringComparison.OrdinalIgnoreCase);
}
