using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Caching;

public interface IChengYuanCache
{
    ValueTask<T?> GetAsync<T>(ChengYuanCacheKey key, CancellationToken cancellationToken = default);

    ValueTask SetAsync<T>(
        ChengYuanCacheKey key,
        T value,
        ChengYuanCacheEntryOptions? options = null,
        CancellationToken cancellationToken = default);

    ValueTask<bool> ExistsAsync(ChengYuanCacheKey key, CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(ChengYuanCacheKey key, CancellationToken cancellationToken = default);

    ValueTask<T> GetOrCreateAsync<T>(
        ChengYuanCacheKey key,
        Func<CancellationToken, ValueTask<T>> factory,
        ChengYuanCacheEntryOptions? options = null,
        CancellationToken cancellationToken = default);
}
