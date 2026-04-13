using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Core.Entities;

namespace ChengYuan.Core.Data;

public sealed class NullDomainEventPublisher : IDomainEventPublisher
{
    public ValueTask PublishAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }
}
