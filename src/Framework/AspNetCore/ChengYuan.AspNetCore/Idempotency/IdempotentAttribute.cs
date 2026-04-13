using System;

namespace ChengYuan.AspNetCore;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class IdempotentAttribute : Attribute
{
    public string HeaderName { get; set; } = "Idempotency-Key";

    public int CacheSeconds { get; set; } = 86400;
}
