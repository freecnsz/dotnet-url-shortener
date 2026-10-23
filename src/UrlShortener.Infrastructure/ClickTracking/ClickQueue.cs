using System.Threading.Channels;
using UrlShortener.Application.Abstractions.Services;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.ClickTracking;

public sealed class ClickQueue : IClickRecorder
{
    private const int Capacity = 10_000;

    private readonly Channel<ClickEvent> _channel = Channel.CreateBounded<ClickEvent>(
        new BoundedChannelOptions(Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

    private long _droppedCount;

    public ChannelReader<ClickEvent> Reader => _channel.Reader;

    public void Record(ClickEvent click)
    {
        if (!_channel.Writer.TryWrite(click))
            Interlocked.Increment(ref _droppedCount);
    }

    public long TakeDroppedCount() => Interlocked.Exchange(ref _droppedCount, 0);
}
