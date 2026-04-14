using System;

namespace ChengYuan.Caching;

public sealed record ChengYuanCacheEntryOptions
{
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; init; }

    public TimeSpan? SlidingExpiration { get; init; }
}
