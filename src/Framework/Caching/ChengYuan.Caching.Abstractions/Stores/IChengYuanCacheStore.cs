using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Caching;

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
