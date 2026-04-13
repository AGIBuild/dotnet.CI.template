using System.Collections.Generic;
using System.Linq;
using ChengYuan.Core.Data;
using ChengYuan.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChengYuan.EntityFrameworkCore;

public sealed class DbContextUnitOfWork(DbContext dbContext) : IUnitOfWork, IDbContextUnitOfWorkParticipant
{
    public IReadOnlyList<IDomainEvent> CollectDomainEvents()
    {
        var trackedEntities = dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToList();

        List<IDomainEvent> events = [];

        foreach (var entity in trackedEntities)
        {
            events.AddRange(entity.DomainEvents);
            entity.ClearDomainEvents();
        }

        return events;
    }

    public ValueTask SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask(dbContext.SaveChangesAsync(cancellationToken));
    }
}
