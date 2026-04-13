using ChengYuan.Core.Entities;

namespace ChengYuan.EntityFrameworkCore;

internal interface IDbContextUnitOfWorkParticipant
{
    IReadOnlyList<IDomainEvent> CollectDomainEvents();

    ValueTask SaveChangesAsync(CancellationToken cancellationToken = default);
}
