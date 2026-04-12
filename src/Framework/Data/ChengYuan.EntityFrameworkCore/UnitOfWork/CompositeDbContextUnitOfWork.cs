using ChengYuan.Core.Data;

namespace ChengYuan.EntityFrameworkCore;

internal sealed class CompositeDbContextUnitOfWork(IEnumerable<IDbContextUnitOfWorkParticipant> participants) : IUnitOfWork
{
    public async ValueTask SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var participant in participants)
        {
            await participant.SaveChangesAsync(cancellationToken);
        }
    }
}
