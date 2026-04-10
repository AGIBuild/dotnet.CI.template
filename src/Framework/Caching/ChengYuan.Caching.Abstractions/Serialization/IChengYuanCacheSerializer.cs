namespace ChengYuan.Caching;

public interface IChengYuanCacheSerializer
{
    ChengYuanCacheItem Serialize<T>(T value);

    T? Deserialize<T>(ChengYuanCacheItem item);
}
