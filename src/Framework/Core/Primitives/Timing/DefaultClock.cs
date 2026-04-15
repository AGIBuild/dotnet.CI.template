using System;

namespace ChengYuan.Core.Timing;

public sealed class DefaultClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
