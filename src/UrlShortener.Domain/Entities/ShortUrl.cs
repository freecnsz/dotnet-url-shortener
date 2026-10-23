using UrlShortener.Domain.ValueObjects;

namespace UrlShortener.Domain.Entities;

public sealed class ShortUrl
{
    public const int ShortCodeLength = 7;
    public const int ManageTokenHashLength = 32; // SHA-256 output size

    public Guid Id { get; private set; }
    public string ShortCode { get; private set; } = null!;
    public OriginalUrl OriginalUrl { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
    public byte[] ManageTokenHash { get; private set; } = null!;

    private ShortUrl() { }

    public static ShortUrl Create(
        OriginalUrl originalUrl,
        string shortCode,
        byte[] manageTokenHash,
        DateTime nowUtc,
        TimeSpan lifetime)
    {
        if (!IsValidShortCode(shortCode))
            throw new ArgumentException(
                $"Short code must be {ShortCodeLength} alphanumeric characters.", nameof(shortCode));

        if (manageTokenHash.Length != ManageTokenHashLength)
            throw new ArgumentException(
                $"Manage token hash must be {ManageTokenHashLength} bytes.", nameof(manageTokenHash));

        if (lifetime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(lifetime), "Lifetime must be positive.");

        return new ShortUrl
        {
            Id = Guid.CreateVersion7(),
            ShortCode = shortCode,
            OriginalUrl = originalUrl,
            CreatedAtUtc = nowUtc,
            ExpiresAtUtc = nowUtc + lifetime,
            ManageTokenHash = manageTokenHash
        };
    }

    public static bool IsValidShortCode(string? value)
        => value is { Length: ShortCodeLength } && value.All(char.IsAsciiLetterOrDigit);

    public bool IsExpired(DateTime nowUtc) => nowUtc >= ExpiresAtUtc;

    public void Delete(DateTime nowUtc) => DeletedAtUtc ??= nowUtc;
}
