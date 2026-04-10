namespace ChengYuan.Core.EntityFrameworkCore;

internal interface IDbContextUnitOfWorkParticipant
{
    ValueTask SaveChangesAsync(CancellationToken cancellationToken = default);
}
