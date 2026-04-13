using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace ChengYuan.Caching;

internal sealed class DistributedCacheStore(IDistributedCache distributedCache) : IChengYuanCacheStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async ValueTask<ChengYuanCacheItem?> GetAsync(string normalizedKey, CancellationToken cancellationToken = default)
    {
        var data = await distributedCache.GetAsync(normalizedKey, cancellationToken);

        if (data is null || data.Length == 0)
        {
            return null;
        }

        var envelope = JsonSerializer.Deserialize<CacheItemEnvelope>(data, SerializerOptions);
        return envelope is not null
            ? new ChengYuanCacheItem(envelope.Payload, envelope.TypeName)
            : null;
    }

    public async ValueTask SetAsync(
        string normalizedKey,
        ChengYuanCacheItem item,
        ChengYuanCacheEntryOptions options,
        CancellationToken cancellationToken = default)
    {
        var envelope = new CacheItemEnvelope(item.TypeName, item.Payload);
        var data = JsonSerializer.SerializeToUtf8Bytes(envelope, SerializerOptions);

        var distributedOptions = new DistributedCacheEntryOptions();

        if (options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            distributedOptions.AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow;
        }

        if (options.SlidingExpiration.HasValue)
        {
            distributedOptions.SlidingExpiration = options.SlidingExpiration;
        }

        await distributedCache.SetAsync(normalizedKey, data, distributedOptions, cancellationToken);
    }

    public async ValueTask RemoveAsync(string normalizedKey, CancellationToken cancellationToken = default)
    {
        await distributedCache.RemoveAsync(normalizedKey, cancellationToken);
    }

    private sealed record CacheItemEnvelope(string TypeName, byte[] Payload);
}
