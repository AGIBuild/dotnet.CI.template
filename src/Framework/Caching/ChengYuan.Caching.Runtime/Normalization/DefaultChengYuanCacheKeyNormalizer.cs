using System;
using ChengYuan.MultiTenancy;

namespace ChengYuan.Caching;

internal sealed class DefaultChengYuanCacheKeyNormalizer(ICurrentTenant currentTenant) : IChengYuanCacheKeyNormalizer
{
    public string Normalize(ChengYuanCacheKey key)
    {
        return key.Scope switch
        {
            ChengYuanCacheScope.Global => $"global::{key.Value}",
            ChengYuanCacheScope.Tenant => currentTenant.Id is Guid tenantId
                ? $"tenant::{tenantId:N}::{key.Value}"
                : throw new InvalidOperationException("Tenant-scoped cache access requires an active tenant context."),
            _ => throw new InvalidOperationException($"Unsupported cache scope '{key.Scope}'.")
        };
    }
}
