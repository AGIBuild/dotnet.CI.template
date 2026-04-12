using System;
using System.Collections.Generic;

namespace ChengYuan.Caching;

public sealed class ChengYuanCacheOptions
{
    public bool HideErrors { get; set; } = true;

    public string KeyPrefix { get; set; } = string.Empty;

    public ChengYuanCacheEntryOptions GlobalCacheEntryOptions { get; set; } = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(20)
    };

    public List<Func<string, ChengYuanCacheEntryOptions?>> CacheConfigurators { get; } = [];

    public void ConfigureCache<TCacheItem>(ChengYuanCacheEntryOptions options)
    {
        ConfigureCache(CacheNameAttribute.GetCacheName<TCacheItem>(), options);
    }

    public void ConfigureCache(string cacheName, ChengYuanCacheEntryOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheName);
        ArgumentNullException.ThrowIfNull(options);

        CacheConfigurators.Add(name => string.Equals(cacheName, name, StringComparison.Ordinal) ? options : null);
    }
}
