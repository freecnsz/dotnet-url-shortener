using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UrlShortener.Application.Options;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Infrastructure.Jobs;

public sealed class ExpiredLinkCleanupJob(
    AppDbContext db,
    TimeProvider timeProvider,
    IOptions<ShortUrlPolicyOptions> policyOptions,
    ILogger<ExpiredLinkCleanupJob> logger)
{
    public const string JobId = "expired-link-cleanup";

    public async Task RunAsync(CancellationToken ct)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var cutoff = nowUtc - policyOptions.Value.StatsRetentionAfterExpiry;

        var deleted = await db.ShortUrls
            .Where(x => x.DeletedAtUtc == null && x.ExpiresAtUtc <= cutoff)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.DeletedAtUtc, nowUtc), ct);

        if (deleted > 0)
            logger.LogInformation("Expired links soft-deleted. Count: {Count}", deleted);
    }
}
