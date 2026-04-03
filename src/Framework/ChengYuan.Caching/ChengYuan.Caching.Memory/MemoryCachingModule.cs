using ChengYuan.Core.Modularity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Caching;

[DependsOn(typeof(CachingModule))]
public sealed class MemoryCachingModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<IChengYuanCacheStore, MemoryCacheStore>();
    }
}

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
