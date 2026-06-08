using ChengYuan.Authorization;
using ChengYuan.Core.Data;
using ChengYuan.MultiTenancy;

namespace ChengYuan.PermissionManagement;

public sealed class PermissionGrantManager(
    IPermissionGrantStore store,
    ITenantScopeAccessPolicy tenantScopeAccessPolicy,
    IUnitOfWork unitOfWork) : IPermissionGrantManager
{
    public async ValueTask<IReadOnlyList<PermissionGrantRecord>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var records = await store.GetListAsync(cancellationToken);
        return tenantScopeAccessPolicy.FilterAccessible(records, record => record.TenantId);
    }

    public async ValueTask SetAsync(PermissionGrantRecord record, CancellationToken cancellationToken = default)
    {
        tenantScopeAccessPolicy.EnsureCanAccess(record.TenantId);
        await store.SetAsync(record, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask RemoveAsync(
        string name,
        PermissionScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        tenantScopeAccessPolicy.EnsureCanAccess(tenantId);
        await store.RemoveAsync(name, scope, tenantId, userId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
