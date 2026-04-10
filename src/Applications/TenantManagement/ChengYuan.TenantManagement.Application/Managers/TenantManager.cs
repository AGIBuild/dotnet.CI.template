using ChengYuan.Core.Guids;

namespace ChengYuan.TenantManagement;

public sealed class TenantManager(ITenantStore store, IGuidGenerator guidGenerator) : ITenantManager
{
    public async ValueTask<TenantRecord> CreateAsync(string name, bool isActive = true, CancellationToken cancellationToken = default)
    {
        var tenant = new TenantRecord(guidGenerator.Create(), name, isActive);
        await store.SetAsync(tenant, cancellationToken);
        return tenant;
    }

    public ValueTask SetAsync(TenantRecord tenant, CancellationToken cancellationToken = default)
    {
        return store.SetAsync(tenant, cancellationToken);
    }

    public ValueTask RemoveAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return store.RemoveAsync(tenantId, cancellationToken);
    }
}
