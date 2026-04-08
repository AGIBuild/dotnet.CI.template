namespace ChengYuan.Caching;

public interface IChengYuanCacheKeyNormalizer
{
    string Normalize(ChengYuanCacheKey key);
}
