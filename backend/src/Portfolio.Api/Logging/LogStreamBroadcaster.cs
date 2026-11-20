using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.Extensions.Configuration;

namespace Portfolio.Api.Logging;

public sealed class LogStreamBroadcaster(IConfiguration? configuration = null)
{
    private const int MaxHistoryEntries = 50;
    private const int SubscriberChannelCapacity = 100;
    private const int DefaultMaxGlobalSubscribers = 50;
    private const int DefaultMaxConnectionDurationSeconds = 600;

    private readonly int _maxGlobalSubscribers =
        Math.Max(1, configuration?.GetValue("RateLimiting:LogStream:MaxGlobalSubscribers", DefaultMaxGlobalSubscribers) ?? DefaultMaxGlobalSubscribers);

    private readonly TimeSpan _maxConnectionDuration = TimeSpan.FromSeconds(
        Math.Max(1, configuration?.GetValue("RateLimiting:LogStream:MaxConnectionDurationSeconds", DefaultMaxConnectionDurationSeconds) ?? DefaultMaxConnectionDurationSeconds));

    private readonly Lock _historyLock = new();
    private readonly Queue<LogStreamEntry> _history = new(MaxHistoryEntries);
    private readonly ConcurrentDictionary<Guid, Channel<LogStreamEntry>> _subscribers = new();

    public TimeSpan MaxConnectionDuration => _maxConnectionDuration;

    public int ActiveSubscribersCount => _subscribers.Count;

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

    public bool TryCreateSubscription(
        out IAsyncEnumerable<LogStreamEntry> stream)
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
            if (_subscribers.Count >= _maxGlobalSubscribers)
            {
                stream = AsyncEnumerable.Empty<LogStreamEntry>();
                return false;
            }

            snapshot = [.. _history];
            _subscribers[subscriberId] = channel;
        }

        stream = ReadSubscriptionAsync(subscriberId, channel, snapshot);
        return true;
    }

    private async IAsyncEnumerable<LogStreamEntry> ReadSubscriptionAsync(
        Guid subscriberId,
        Channel<LogStreamEntry> channel,
        List<LogStreamEntry> snapshot,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
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
