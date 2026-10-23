namespace UrlShortener.Application.Features.ShortUrls.ShortenUrl;

public sealed record ShortenUrlResponse(
    string ShortUrl,
    byte[] QRCode,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    string ManageToken);
