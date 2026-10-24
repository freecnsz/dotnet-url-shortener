using UrlShortener.Domain.ValueObjects;

namespace UrlShortener.Application.Abstractions.Services;

public interface IAbsoluteUrlBuilder
{
    string BuildAbsoluteUrl(string shortCode);
    bool PointsToSelf(OriginalUrl url);
}
