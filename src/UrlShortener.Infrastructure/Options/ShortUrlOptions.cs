namespace UrlShortener.Infrastructure.Options;

public sealed class ShortUrlOptions
{
    public const string SectionName = "ShortUrlOptions";

    public string BaseUrl { get; init; } = string.Empty;
}
