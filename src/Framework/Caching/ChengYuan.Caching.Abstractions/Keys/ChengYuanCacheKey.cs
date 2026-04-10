using System;

namespace ChengYuan.Caching;

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
