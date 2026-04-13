using System;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Core.Data;
using ChengYuan.Core.Entities;

namespace ChengYuan.Caching;

internal sealed class CachingEntityCache<TEntity, TId>(
    IChengYuanCache<TEntity> cache,
    IReadOnlyRepository<TEntity, TId> repository) : IEntityCache<TEntity, TId>
    where TEntity : class, IAggregateRoot<TId>
    where TId : notnull
{
    public async ValueTask<TEntity?> FindAsync(TId id, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(id);

        return await cache.GetOrCreateAsync(
            cacheKey,
            async ct => (await repository.FindAsync(id, ct))!,
            cancellationToken: cancellationToken);
    }

    public async ValueTask<TEntity> GetAsync(TId id, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(id);

        return await cache.GetOrCreateAsync(
            cacheKey,
            ct => new ValueTask<TEntity>(repository.GetAsync(id, ct).AsTask()),
            cancellationToken: cancellationToken);
    }

    public async ValueTask InvalidateAsync(TId id, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(id);
        await cache.RemoveAsync(cacheKey, cancellationToken);
    }

    private static string BuildCacheKey(TId id) => $"{typeof(TEntity).Name}:{id}";
}
