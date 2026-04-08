using System;
using ChengYuan.Core.Timing;

namespace ChengYuan.ExecutionContext;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
