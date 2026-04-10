using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace ChengYuan.Caching;

internal sealed class MemoryCacheStore(IMemoryCache memoryCache) : IChengYuanCacheStore
{
    public ValueTask<ChengYuanCacheItem?> GetAsync(string normalizedKey, CancellationToken cancellationToken = default)
    {
        memoryCache.TryGetValue(normalizedKey, out ChengYuanCacheItem? item);
        return ValueTask.FromResult(item);
    }

    public ValueTask SetAsync(
        string normalizedKey,
        ChengYuanCacheItem item,
        ChengYuanCacheEntryOptions options,
        CancellationToken cancellationToken = default)
    {
        var entryOptions = new MemoryCacheEntryOptions();

        if (options.AbsoluteExpirationRelativeToNow.HasValue)
            entryOptions.AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow;

        if (options.SlidingExpiration.HasValue)
            entryOptions.SlidingExpiration = options.SlidingExpiration;

        memoryCache.Set(normalizedKey, item, entryOptions);
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(string normalizedKey, CancellationToken cancellationToken = default)
    {
        memoryCache.Remove(normalizedKey);
        return ValueTask.CompletedTask;
    }
}
