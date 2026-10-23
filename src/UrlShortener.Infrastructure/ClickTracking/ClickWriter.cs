using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Infrastructure.ClickTracking;

public sealed class ClickWriter(
    ClickQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<ClickWriter> logger) : BackgroundService
{
    private const int MaxBatchSize = 500;
    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (await queue.Reader.WaitToReadAsync(stoppingToken))
            {
                await Task.Delay(FlushInterval, stoppingToken);
                await FlushAsync();
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }

        await FlushAsync();
    }

    private async Task FlushAsync()
    {
        ReportDroppedClicks();

        var batch = new List<ClickEvent>(MaxBatchSize);
        while (true)
        {
            batch.Clear();
            while (batch.Count < MaxBatchSize && queue.Reader.TryRead(out var click))
                batch.Add(click);

            if (batch.Count == 0)
                return;

            await WriteBatchAsync(batch);
        }
    }

    private async Task WriteBatchAsync(List<ClickEvent> batch)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.ClickEvents.AddRange(batch);
            await db.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write click batch. Lost clicks: {Count}", batch.Count);
        }
    }

    private void ReportDroppedClicks()
    {
        var dropped = queue.TakeDroppedCount();
        if (dropped > 0)
            logger.LogWarning("Click queue was full. Dropped clicks: {Count}", dropped);
    }
}
