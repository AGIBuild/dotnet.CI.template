using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Core.Entities;

namespace ChengYuan.Core.Data;

public interface IEntityCache<TEntity, in TId>
    where TEntity : class, IAggregateRoot<TId>
    where TId : notnull
{
    ValueTask<TEntity?> FindAsync(TId id, CancellationToken cancellationToken = default);

    ValueTask<TEntity> GetAsync(TId id, CancellationToken cancellationToken = default);

    ValueTask InvalidateAsync(TId id, CancellationToken cancellationToken = default);
}
