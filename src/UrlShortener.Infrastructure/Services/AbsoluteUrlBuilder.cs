using Microsoft.Extensions.Options;
using UrlShortener.Application.Abstractions.Services;
using UrlShortener.Infrastructure.Options;

namespace UrlShortener.Infrastructure.Services;

public sealed class AbsoluteUrlBuilder(IOptions<ShortUrlOptions> options) : IAbsoluteUrlBuilder
{
    private readonly string _baseUrl = options.Value.BaseUrl.TrimEnd('/');

    public string BuildAbsoluteUrl(string shortCode) => $"{_baseUrl}/{shortCode}";
}
