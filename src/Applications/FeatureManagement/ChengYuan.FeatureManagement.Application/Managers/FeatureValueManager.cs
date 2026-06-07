using ChengYuan.Features;
using ChengYuan.MultiTenancy;

namespace ChengYuan.FeatureManagement;

public sealed class FeatureValueManager(
    IFeatureValueStore store,
    ITenantScopeAccessPolicy tenantScopeAccessPolicy) : IFeatureValueManager
{
    public async ValueTask<IReadOnlyList<FeatureValueRecord>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var records = await store.GetListAsync(cancellationToken);
        return tenantScopeAccessPolicy.FilterAccessible(records, record => record.TenantId);
    }

    public ValueTask SetAsync(FeatureValueRecord record, CancellationToken cancellationToken = default)
    {
        tenantScopeAccessPolicy.EnsureCanAccess(record.TenantId);
        return store.SetAsync(record, cancellationToken);
    }

    public ValueTask RemoveAsync(
        string name,
        FeatureScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        tenantScopeAccessPolicy.EnsureCanAccess(tenantId);
        return store.RemoveAsync(name, scope, tenantId, userId, cancellationToken);
    }
}
