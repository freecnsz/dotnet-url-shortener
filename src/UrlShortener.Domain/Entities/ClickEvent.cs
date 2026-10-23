namespace UrlShortener.Domain.Entities;

public sealed class ClickEvent
{
    public const int CountryCodeLength = 2;
    public const int RefererHostMaxLength = 255;
    public const int UserAgentMaxLength = 512;

    public long Id { get; private set; }
    public Guid ShortUrlId { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public string? CountryCode { get; private set; }
    public string? RefererHost { get; private set; }
    public string? UserAgent { get; private set; }

    private ClickEvent() { }

    public static ClickEvent Create(
        Guid shortUrlId,
        DateTime occurredAtUtc,
        string? countryCode,
        string? refererHost,
        string? userAgent)
    {
        if (shortUrlId == Guid.Empty)
            throw new ArgumentException("ShortUrlId must not be empty.", nameof(shortUrlId));

        if (occurredAtUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Timestamp must be UTC.", nameof(occurredAtUtc));

        return new ClickEvent
        {
            ShortUrlId = shortUrlId,
            OccurredAtUtc = occurredAtUtc,
            CountryCode = NormalizeCountryCode(countryCode),
            RefererHost = NormalizeRefererHost(refererHost),
            UserAgent = Truncate(userAgent, UserAgentMaxLength)
        };
    }
    private static string? NormalizeCountryCode(string? value)
        => value is { Length: CountryCodeLength } && value.All(char.IsAsciiLetter)
            ? value.ToUpperInvariant()
            : null;

    private static string? NormalizeRefererHost(string? value)
        => string.IsNullOrWhiteSpace(value) || value.Length > RefererHostMaxLength
            ? null
            : value.ToLowerInvariant();

    private static string? Truncate(string? value, int maxLength)
        => string.IsNullOrEmpty(value) ? null
         : value.Length <= maxLength ? value
         : value[..maxLength];
}
