using ChengYuan.Core.Data;
using ChengYuan.Features;
using ChengYuan.MultiTenancy;

namespace ChengYuan.FeatureManagement;

public sealed class FeatureValueManager(
    IFeatureValueStore store,
    ITenantScopeAccessPolicy tenantScopeAccessPolicy,
    IUnitOfWork unitOfWork) : IFeatureValueManager
{
    public async ValueTask<IReadOnlyList<FeatureValueRecord>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var records = await store.GetListAsync(cancellationToken);
        return tenantScopeAccessPolicy.FilterAccessible(records, record => record.TenantId);
    }

    public async ValueTask SetAsync(FeatureValueRecord record, CancellationToken cancellationToken = default)
    {
        tenantScopeAccessPolicy.EnsureCanAccess(record.TenantId);
        await store.SetAsync(record, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask RemoveAsync(
        string name,
        FeatureScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        tenantScopeAccessPolicy.EnsureCanAccess(tenantId);
        await store.RemoveAsync(name, scope, tenantId, userId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
