using System.Diagnostics.CodeAnalysis;

namespace UrlShortener.Domain.ValueObjects;

public sealed record OriginalUrl
{
    public const int MaxLength = 2048;

    public string Value { get; }

    private OriginalUrl(string value) => Value = value;

    public static bool TryCreate(string? value, [NotNullWhen(true)] out OriginalUrl? result)
    {
        result = null;

        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > MaxLength)
            return false;

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return false;

        // Reject "https://trusted.com@evil.com" style URLs used for phishing
        if (!string.IsNullOrEmpty(uri.UserInfo))
            return false;

        result = new OriginalUrl(trimmed);
        return true;
    }

    public static OriginalUrl Create(string value)
        => TryCreate(value, out var result)
            ? result
            : throw new ArgumentException("A valid absolute http/https URL is required.", nameof(value));

    public override string ToString() => Value;
}
