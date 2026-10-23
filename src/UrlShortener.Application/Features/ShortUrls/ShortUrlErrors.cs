using UrlShortener.Application.Common.Results;

namespace UrlShortener.Application.Features.ShortUrls;

public static class ShortUrlErrors
{
    public static readonly Error InvalidUrl = Error.Validation(
        "Url.Invalid", "A valid HTTP or HTTPS URL is required.");

    public static readonly Error InvalidTimeZone = Error.Validation(
        "TimeZone.Invalid", "Time zone must be a valid IANA id, e.g. Europe/Istanbul.");

    public static readonly Error InvalidManageToken = Error.Unauthorized(
        "ManageToken.Invalid", "The manage token is missing or invalid.");

    public static readonly Error NotFound = Error.NotFound(
        "ShortCode.NotFound", "No URL found for this short code.");

    public static readonly Error Expired = Error.Gone(
        "ShortCode.Expired", "This short URL has expired.");

    public static readonly Error ShortCodeCollision = Error.Failure(
        "ShortCode.Collision", "Short code collision occurred. Please retry.");
}
