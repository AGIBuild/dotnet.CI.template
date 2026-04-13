using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Core.Entities;

namespace ChengYuan.Core.Data;

public interface IDomainEventPublisher
{
    ValueTask PublishAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
