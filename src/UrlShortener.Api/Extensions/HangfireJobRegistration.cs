using Hangfire;
using UrlShortener.Infrastructure.Jobs;

namespace UrlShortener.Api.Extensions;

public static class HangfireJobRegistration
{
    public static void RegisterRecurringJobs(this IApplicationBuilder app)
    {
        var manager = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();

        // Hangfire substitutes its own cancellation token at execution time
        manager.AddOrUpdate<ExpiredLinkCleanupJob>(
            ExpiredLinkCleanupJob.JobId,
            job => job.RunAsync(CancellationToken.None),
            Cron.Hourly());
    }
}
