using ChengYuan.Settings;
using ChengYuan.MultiTenancy;

namespace ChengYuan.SettingManagement;

public sealed class SettingValueManager(
    ISettingValueStore store,
    ITenantScopeAccessPolicy tenantScopeAccessPolicy) : ISettingValueManager
{
    public async ValueTask<IReadOnlyList<SettingValueRecord>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var records = await store.GetListAsync(cancellationToken);
        return tenantScopeAccessPolicy.FilterAccessible(records, record => record.TenantId);
    }

    public ValueTask SetAsync(SettingValueRecord record, CancellationToken cancellationToken = default)
    {
        tenantScopeAccessPolicy.EnsureCanAccess(record.TenantId);
        return store.SetAsync(record, cancellationToken);
    }

    public ValueTask RemoveAsync(
        string name,
        SettingScope scope,
        Guid? tenantId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        tenantScopeAccessPolicy.EnsureCanAccess(tenantId);
        return store.RemoveAsync(name, scope, tenantId, userId, cancellationToken);
    }
}
