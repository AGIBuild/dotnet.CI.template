using ChengYuan.Authorization;
using ChengYuan.MultiTenancy;

namespace ChengYuan.PermissionManagement;

public sealed class PermissionGrantManager(
    IPermissionGrantStore store,
    ITenantScopeAccessPolicy tenantScopeAccessPolicy) : IPermissionGrantManager
{
    public async ValueTask<IReadOnlyList<PermissionGrantRecord>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var records = await store.GetListAsync(cancellationToken);
        return tenantScopeAccessPolicy.FilterAccessible(records, record => record.TenantId);
    }

    public ValueTask SetAsync(PermissionGrantRecord record, CancellationToken cancellationToken = default)
    {
        tenantScopeAccessPolicy.EnsureCanAccess(record.TenantId);
        return store.SetAsync(record, cancellationToken);
    }

    public ValueTask RemoveAsync(
        string name,
        PermissionScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        tenantScopeAccessPolicy.EnsureCanAccess(tenantId);
        return store.RemoveAsync(name, scope, tenantId, userId, cancellationToken);
    }
}
