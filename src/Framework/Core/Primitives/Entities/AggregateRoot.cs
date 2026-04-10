using System;
using System.Collections.Generic;

namespace ChengYuan.Core.Entities;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot<TId>, IHasDomainEvents
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot()
    {
    }

    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }
}
