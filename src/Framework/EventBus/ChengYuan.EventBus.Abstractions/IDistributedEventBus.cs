using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.EventBus;

public interface IDistributedEventBus
{
    ValueTask PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default)
        where TEvent : class;

    ValueTask PublishAsync(Type eventType, object eventData, CancellationToken cancellationToken = default);
}
