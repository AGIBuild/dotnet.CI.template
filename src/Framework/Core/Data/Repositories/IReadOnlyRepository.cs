using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Core.Entities;

namespace ChengYuan.Core.Data;

public interface IReadOnlyRepository<TEntity, in TId>
    where TEntity : class, IAggregateRoot<TId>
    where TId : notnull
{
    ValueTask<TEntity?> FindAsync(TId id, CancellationToken cancellationToken = default);

    ValueTask<TEntity> GetAsync(TId id, CancellationToken cancellationToken = default);

    ValueTask<List<TEntity>> GetPagedListAsync(int skipCount, int maxResultCount, string? sorting = null, CancellationToken cancellationToken = default);

    ValueTask<long> GetCountAsync(CancellationToken cancellationToken = default);
}
