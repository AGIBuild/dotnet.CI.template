using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.EventBus;

public sealed class LocalDistributedEventBus(ILocalEventBus localEventBus) : IDistributedEventBus
{
    public ValueTask PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(eventData);

        return localEventBus.PublishAsync(eventData, cancellationToken);
    }

    public ValueTask PublishAsync(Type eventType, object eventData, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        ArgumentNullException.ThrowIfNull(eventData);

        return localEventBus.PublishAsync(eventType, eventData, cancellationToken);
    }
}
