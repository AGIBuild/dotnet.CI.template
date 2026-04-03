using ChengYuan.Core.Entities;

namespace ChengYuan.Core.Data;

public interface IReadOnlyRepository<TEntity, in TId>
    where TEntity : class, IAggregateRoot<TId>
    where TId : notnull
{
    ValueTask<TEntity?> FindAsync(TId id, CancellationToken cancellationToken = default);

    ValueTask<TEntity> GetAsync(TId id, CancellationToken cancellationToken = default);
}

public interface IRepository<TEntity, in TId> : IReadOnlyRepository<TEntity, TId>
    where TEntity : class, IAggregateRoot<TId>
    where TId : notnull
{
    ValueTask<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    ValueTask DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}
