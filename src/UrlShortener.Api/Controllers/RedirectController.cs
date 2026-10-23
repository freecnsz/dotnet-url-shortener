using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UrlShortener.Api.Extensions;
using UrlShortener.Api.Options;
using UrlShortener.Application.Abstractions.Mediator;
using UrlShortener.Application.Features.ShortUrls.RedirectShortUrl;

namespace UrlShortener.Api.Controllers;

[ApiController]
public sealed class RedirectController(
    ISender sender,
    IOptions<ClickTrackingOptions> clickTrackingOptions) : ControllerBase
{
    private const string CloudflareCountryHeader = "CF-IPCountry";

    [HttpGet("/{shortCode}")]
    public async Task<IActionResult> Follow(string shortCode, CancellationToken ct)
    {
        var query = new RedirectShortUrlQuery(shortCode, ReadClickMetadata());
        var result = await sender.Send(query, ct);

        return result.IsSuccess
            ? Redirect(result.Value.OriginalUrl)
            : result.ToActionResult(this);
    }

    private ClickMetadata ReadClickMetadata()
    {
        var headers = Request.Headers;

        var refererHost = Uri.TryCreate(headers.Referer.ToString(), UriKind.Absolute, out var referer)
            ? referer.Host
            : null;

        var countryCode = clickTrackingOptions.Value.TrustCloudflareHeaders
            ? headers[CloudflareCountryHeader].ToString()
            : null;

        return new ClickMetadata(refererHost, headers.UserAgent.ToString(), countryCode);
    }
}
