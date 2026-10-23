namespace UrlShortener.Infrastructure.Options;

public sealed class UrlCacheOptions
{
    public const string SectionName = "UrlCacheOptions";

    public TimeSpan Ttl { get; init; } = TimeSpan.FromHours(1);
}
