using System;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Outbox;

namespace ChengYuan.EventBus.Outbox;

public sealed class OutboxDistributedEventBus(IOutbox outbox) : IDistributedEventBus
{
    public ValueTask PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(eventData);

        var eventName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
        return new ValueTask(outbox.EnqueueAsync(eventName, eventData, cancellationToken).AsTask());
    }

    public async ValueTask PublishAsync(Type eventType, object eventData, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        ArgumentNullException.ThrowIfNull(eventData);

        var eventName = eventType.FullName ?? eventType.Name;
        await outbox.EnqueueAsync(eventName, eventData, cancellationToken);
    }
}
