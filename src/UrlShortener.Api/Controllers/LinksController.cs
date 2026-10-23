using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UrlShortener.Api.Contracts;
using UrlShortener.Api.Extensions;
using UrlShortener.Application.Abstractions.Mediator;
using UrlShortener.Application.Features.ShortUrls.DeleteLink;
using UrlShortener.Application.Features.ShortUrls.GetLinkStats;
using UrlShortener.Application.Features.ShortUrls.ShortenUrl;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/links")]
public sealed class LinksController(ISender sender) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting(RateLimitingExtensions.CreateLinkPolicy)]
    public async Task<IActionResult> Create([FromBody] ShortenUrlRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ShortenUrlCommand(request.OriginalUrl), ct);
        return result.ToCreatedResult(this, response => response.ShortUrl);
    }

    [HttpGet("{shortCode}/stats")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetStats(
        string shortCode,
        [FromHeader(Name = Headers.ManageToken)] string? manageToken,
        [FromQuery] string? timeZone,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetLinkStatsQuery(shortCode, manageToken, timeZone), ct);
        return result.ToActionResult(this);
    }

    [HttpDelete("{shortCode}")]
    public async Task<IActionResult> Delete(
        string shortCode,
        [FromHeader(Name = Headers.ManageToken)] string? manageToken,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteLinkCommand(shortCode, manageToken), ct);
        return result.ToNoContentResult(this);
    }
}
