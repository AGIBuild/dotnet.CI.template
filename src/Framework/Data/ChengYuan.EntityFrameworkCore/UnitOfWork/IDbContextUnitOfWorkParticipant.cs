using ChengYuan.Core.Entities;

namespace ChengYuan.EntityFrameworkCore;

internal interface IDbContextUnitOfWorkParticipant
{
    IReadOnlyList<IDomainEvent> CollectDomainEvents();

    ValueTask BeginTransactionAsync(CancellationToken cancellationToken = default);

    ValueTask SaveChangesAsync(CancellationToken cancellationToken = default);

    ValueTask CommitAsync(CancellationToken cancellationToken = default);

    ValueTask RollbackAsync(CancellationToken cancellationToken = default);
}
