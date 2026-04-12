namespace ChengYuan.EntityFrameworkCore;

internal interface IDbContextUnitOfWorkParticipant
{
    ValueTask SaveChangesAsync(CancellationToken cancellationToken = default);
}
