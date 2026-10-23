namespace UrlShortener.Api.Options;

public sealed class ClickTrackingOptions
{
    public const string SectionName = "ClickTracking";
    public bool TrustCloudflareHeaders { get; init; }
}
