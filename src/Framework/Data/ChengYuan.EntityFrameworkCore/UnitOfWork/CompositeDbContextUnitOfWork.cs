using System.Collections.Generic;
using ChengYuan.Core.Data;
using ChengYuan.Core.Entities;

namespace ChengYuan.EntityFrameworkCore;

internal sealed class CompositeDbContextUnitOfWork(
    IEnumerable<IDbContextUnitOfWorkParticipant> participants,
    IDomainEventPublisher domainEventPublisher) : IUnitOfWork
{
    public async ValueTask SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        List<IDomainEvent> allEvents = [];

        foreach (var participant in participants)
        {
            allEvents.AddRange(participant.CollectDomainEvents());
        }

        foreach (var participant in participants)
        {
            await participant.SaveChangesAsync(cancellationToken);
        }

        if (allEvents.Count > 0)
        {
            await domainEventPublisher.PublishAsync(allEvents, cancellationToken);
        }
    }
}
