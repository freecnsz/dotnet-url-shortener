using Microsoft.Extensions.Logging;
using UrlShortener.Application.Abstractions.Mediator;
using UrlShortener.Application.Abstractions.Persistence;
using UrlShortener.Application.Abstractions.Services;
using UrlShortener.Application.Common.Results;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Features.ShortUrls.DeleteLink;

public sealed class DeleteLinkCommandHandler(
    IShortUrlRepository shortUrlRepository,
    IManageTokenService manageTokenService,
    IUrlLookup urlLookup,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<DeleteLinkCommandHandler> logger)
    : IRequestHandler<DeleteLinkCommand, Result>
{
    public async Task<Result> Handle(DeleteLinkCommand command, CancellationToken ct)
    {
        if (!ShortUrl.IsValidShortCode(command.ShortCode))
            return ShortUrlErrors.NotFound;

        var shortUrl = await shortUrlRepository.GetByShortCodeAsync(command.ShortCode, ct);
        if (shortUrl is null || shortUrl.DeletedAtUtc is not null)
            return ShortUrlErrors.NotFound;

        if (!manageTokenService.Verify(command.ManageToken, shortUrl.ManageTokenHash))
        {
            logger.LogInformation("Manage token rejected. ShortCode: {ShortCode}", command.ShortCode);
            return ShortUrlErrors.InvalidManageToken;
        }

        shortUrl.Delete(timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(ct);

        // Only after the commit: the database is the source of truth
        await urlLookup.InvalidateAsync(shortUrl.ShortCode);

        logger.LogInformation("Short URL deleted. ShortCode: {ShortCode}, UrlId: {UrlId}",
            shortUrl.ShortCode, shortUrl.Id);

        return Result.Success();
    }
}
