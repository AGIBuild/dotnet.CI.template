using ChengYuan.Core.Data;
using ChengYuan.Settings;
using ChengYuan.MultiTenancy;

namespace ChengYuan.SettingManagement;

public sealed class SettingValueManager(
    ISettingValueStore store,
    ITenantScopeAccessPolicy tenantScopeAccessPolicy,
    IUnitOfWork unitOfWork) : ISettingValueManager
{
    public async ValueTask<IReadOnlyList<SettingValueRecord>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var records = await store.GetListAsync(cancellationToken);
        return tenantScopeAccessPolicy.FilterAccessible(records, record => record.TenantId);
    }

    public async ValueTask SetAsync(SettingValueRecord record, CancellationToken cancellationToken = default)
    {
        tenantScopeAccessPolicy.EnsureCanAccess(record.TenantId);
        await store.SetAsync(record, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask RemoveAsync(
        string name,
        SettingScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        tenantScopeAccessPolicy.EnsureCanAccess(tenantId);
        await store.RemoveAsync(name, scope, tenantId, userId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
