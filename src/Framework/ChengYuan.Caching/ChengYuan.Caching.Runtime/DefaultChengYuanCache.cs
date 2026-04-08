using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Caching;

internal sealed class DefaultChengYuanCache(
    IChengYuanCacheStore store,
    IChengYuanCacheKeyNormalizer keyNormalizer,
    IChengYuanCacheSerializer serializer) : IChengYuanCache
{
    public async ValueTask<T?> GetAsync<T>(ChengYuanCacheKey key, CancellationToken cancellationToken = default)
    {
        var normalizedKey = keyNormalizer.Normalize(key);
        var item = await store.GetAsync(normalizedKey, cancellationToken);

        return item is null
            ? default
            : serializer.Deserialize<T>(item);
    }

    public async ValueTask SetAsync<T>(
        ChengYuanCacheKey key,
        T value,
        ChengYuanCacheEntryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);

        var normalizedKey = keyNormalizer.Normalize(key);
        var item = serializer.Serialize(value);

        await store.SetAsync(normalizedKey, item, options ?? new ChengYuanCacheEntryOptions(), cancellationToken);
    }

    public async ValueTask<bool> ExistsAsync(ChengYuanCacheKey key, CancellationToken cancellationToken = default)
    {
        var normalizedKey = keyNormalizer.Normalize(key);
        return await store.GetAsync(normalizedKey, cancellationToken) is not null;
    }

    public async ValueTask RemoveAsync(ChengYuanCacheKey key, CancellationToken cancellationToken = default)
    {
        var normalizedKey = keyNormalizer.Normalize(key);
        await store.RemoveAsync(normalizedKey, cancellationToken);
    }

    public async ValueTask<T> GetOrCreateAsync<T>(
        ChengYuanCacheKey key,
        Func<CancellationToken, ValueTask<T>> factory,
        ChengYuanCacheEntryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var normalizedKey = keyNormalizer.Normalize(key);
        var existingItem = await store.GetAsync(normalizedKey, cancellationToken);
        if (existingItem is not null)
            return serializer.Deserialize<T>(existingItem)!;

        var created = await factory(cancellationToken);
        await SetAsync(key, created, options, cancellationToken);

        return created;
    }
}
