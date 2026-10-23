namespace UrlShortener.Application.Options;

public sealed class ShortUrlPolicyOptions
{
    public const string SectionName = "ShortUrlPolicy";

    public TimeSpan DefaultLifetime { get; init; }

    // How long stats stay viewable after a link expires, before it is soft-deleted
    public TimeSpan StatsRetentionAfterExpiry { get; init; }
}
