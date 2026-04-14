using System;

namespace ChengYuan.Caching;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
public sealed class CacheNameAttribute : Attribute
{
    public string Name { get; }

    public CacheNameAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    public static string GetCacheName<TCacheItem>()
    {
        return GetCacheName(typeof(TCacheItem));
    }

    public static string GetCacheName(Type cacheItemType)
    {
        ArgumentNullException.ThrowIfNull(cacheItemType);

        var attribute = (CacheNameAttribute?)Attribute.GetCustomAttribute(cacheItemType, typeof(CacheNameAttribute));

        return attribute is not null
            ? attribute.Name
            : StripCacheItemSuffix(cacheItemType.FullName ?? cacheItemType.Name);
    }

    private static string StripCacheItemSuffix(string typeName)
    {
        const string suffix = "CacheItem";
        return typeName.EndsWith(suffix, StringComparison.Ordinal)
            ? typeName[..^suffix.Length]
            : typeName;
    }
}
