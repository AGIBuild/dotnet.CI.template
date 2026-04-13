using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Core.Entities;

namespace ChengYuan.EventBus;

public sealed class DomainEventDispatcher(ILocalEventBus eventBus)
{
    public async ValueTask DispatchAndClearAsync(IEnumerable<IHasDomainEvents> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var events = new List<IDomainEvent>();

        foreach (var entity in entities)
        {
            events.AddRange(entity.DomainEvents);
            entity.ClearDomainEvents();
        }

        foreach (var domainEvent in events)
        {
            await eventBus.PublishAsync(domainEvent.GetType(), domainEvent, cancellationToken);
        }
    }
}
