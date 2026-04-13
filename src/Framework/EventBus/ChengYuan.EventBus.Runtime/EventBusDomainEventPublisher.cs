using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Core.Data;
using ChengYuan.Core.Entities;

namespace ChengYuan.EventBus;

public sealed class EventBusDomainEventPublisher(ILocalEventBus eventBus) : IDomainEventPublisher
{
    public async ValueTask PublishAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        foreach (var domainEvent in domainEvents)
        {
            await eventBus.PublishAsync(domainEvent.GetType(), domainEvent, cancellationToken);
        }
    }
}
