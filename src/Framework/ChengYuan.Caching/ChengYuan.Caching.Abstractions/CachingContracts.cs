namespace ChengYuan.Caching;

public enum ChengYuanCacheScope
{
    Global = 0,
    Tenant = 1
}

public readonly record struct ChengYuanCacheKey
{
    public ChengYuanCacheKey(string value, ChengYuanCacheScope scope = ChengYuanCacheScope.Global)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Value = value;
        Scope = scope;
    }

    public string Value { get; }

    public ChengYuanCacheScope Scope { get; }
}

public sealed record ChengYuanCacheEntryOptions
{
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; init; }

    public TimeSpan? SlidingExpiration { get; init; }
}

public sealed record ChengYuanCacheItem(byte[] Payload, string TypeName);

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

public interface IChengYuanCacheStore
{
    ValueTask<ChengYuanCacheItem?> GetAsync(string normalizedKey, CancellationToken cancellationToken = default);

    ValueTask SetAsync(
        string normalizedKey,
        ChengYuanCacheItem item,
        ChengYuanCacheEntryOptions options,
        CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(string normalizedKey, CancellationToken cancellationToken = default);
}

public interface IChengYuanCacheKeyNormalizer
{
    string Normalize(ChengYuanCacheKey key);
}

public interface IChengYuanCacheSerializer
{
    ChengYuanCacheItem Serialize<T>(T value);

    T? Deserialize<T>(ChengYuanCacheItem item);
}
