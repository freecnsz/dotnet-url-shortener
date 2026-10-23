using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UrlShortener.Application.Abstractions.Mediator;
using UrlShortener.Application.Abstractions.Persistence;
using UrlShortener.Application.Abstractions.Services;
using UrlShortener.Application.Common.Results;
using UrlShortener.Application.Options;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.ValueObjects;

namespace UrlShortener.Application.Features.ShortUrls.ShortenUrl;

public sealed class ShortenUrlCommandHandler(
    IShortUrlRepository shortUrlRepository,
    IShortCodeGenerator shortCodeGenerator,
    IManageTokenService manageTokenService,
    IAbsoluteUrlBuilder absoluteUrlBuilder,
    IQRCodeGenerator qrCodeGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IOptions<ShortUrlPolicyOptions> policyOptions,
    ILogger<ShortenUrlCommandHandler> logger)
    : IRequestHandler<ShortenUrlCommand, Result<ShortenUrlResponse>>
{
    public async Task<Result<ShortenUrlResponse>> Handle(ShortenUrlCommand command, CancellationToken ct)
    {
        if (!OriginalUrl.TryCreate(command.OriginalUrl, out var originalUrl))
        {
            logger.LogInformation("Short URL rejected: invalid URL. Length: {Length}",
                command.OriginalUrl?.Length ?? 0);
            return ShortUrlErrors.InvalidUrl;
        }

        // Everything that can fail runs before the commit
        var shortCode = shortCodeGenerator.Generate();
        var manageToken = manageTokenService.Generate();
        var absoluteUrl = absoluteUrlBuilder.BuildAbsoluteUrl(shortCode);
        var qrCode = qrCodeGenerator.GeneratePngQRCode(absoluteUrl);

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var shortUrl = ShortUrl.Create(
            originalUrl, shortCode, manageToken.Hash, nowUtc, policyOptions.Value.DefaultLifetime);
        shortUrlRepository.Add(shortUrl);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (DuplicateShortCodeException)
        {
            logger.LogError("Short code collision. ShortCode: {ShortCode}", shortCode);
            return ShortUrlErrors.ShortCodeCollision;
        }

        logger.LogInformation("Short URL created. ShortCode: {ShortCode}, UrlId: {UrlId}",
            shortUrl.ShortCode, shortUrl.Id);

        // The plain token leaves the system only here, once
        return new ShortenUrlResponse(
            absoluteUrl, qrCode, shortUrl.CreatedAtUtc, shortUrl.ExpiresAtUtc, manageToken.Token);
    }
}
