namespace UrlShortener.Application.Abstractions.Services;

public interface IAbsoluteUrlBuilder
{
    string BuildAbsoluteUrl(string shortCode);
}
