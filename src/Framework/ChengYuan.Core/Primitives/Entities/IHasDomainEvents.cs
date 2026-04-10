using System.Collections.Generic;

namespace ChengYuan.Core.Entities;

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
