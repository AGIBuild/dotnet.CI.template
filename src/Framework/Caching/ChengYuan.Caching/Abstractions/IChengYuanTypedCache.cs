using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Caching;

public interface IChengYuanCache<TCacheItem>
    where TCacheItem : class
{
    ValueTask<TCacheItem?> GetAsync(string key, CancellationToken cancellationToken = default);

    ValueTask SetAsync(
        string key,
        TCacheItem value,
        ChengYuanCacheEntryOptions? options = null,
        CancellationToken cancellationToken = default);

    ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default);

    ValueTask<TCacheItem> GetOrCreateAsync(
        string key,
        Func<CancellationToken, ValueTask<TCacheItem>> factory,
        ChengYuanCacheEntryOptions? options = null,
        CancellationToken cancellationToken = default);
}
