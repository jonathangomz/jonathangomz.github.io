using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Portfolio.Api.Logging;

public sealed class LogStreamBroadcaster
{
    private const int MaxHistoryEntries = 50;
    private const int SubscriberChannelCapacity = 100;

    private readonly Lock _historyLock = new();
    private readonly Queue<LogStreamEntry> _history = new(MaxHistoryEntries);
    private readonly ConcurrentDictionary<Guid, Channel<LogStreamEntry>> _subscribers = new();

    public void Publish(LogStreamEntry entry)
    {
        lock (_historyLock)
        {
            if (_history.Count >= MaxHistoryEntries)
            {
                _history.Dequeue();
            }

            _history.Enqueue(entry);
        }

        foreach (var subscriber in _subscribers.Values)
        {
            subscriber.Writer.TryWrite(entry);
        }
    }

    public IReadOnlyList<LogStreamEntry> GetHistorySnapshot()
    {
        lock (_historyLock)
        {
            return [.. _history];
        }
    }

    public async IAsyncEnumerable<LogStreamEntry> SubscribeAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var subscriberId = Guid.NewGuid();
        var channel = Channel.CreateBounded<LogStreamEntry>(new BoundedChannelOptions(SubscriberChannelCapacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });

        List<LogStreamEntry> snapshot;
        lock (_historyLock)
        {
            snapshot = [.. _history];
            _subscribers[subscriberId] = channel;
        }

        try
        {
            foreach (var entry in snapshot)
            {
                yield return entry;
            }

            await foreach (var entry in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return entry;
            }
        }
        finally
        {
            _subscribers.TryRemove(subscriberId, out _);
            channel.Writer.TryComplete();
        }
    }
}
