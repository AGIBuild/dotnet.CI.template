namespace ChengYuan.TenantManagement;

public interface ITenantManager
{
    ValueTask<TenantRecord> CreateAsync(string name, bool isActive = true, CancellationToken cancellationToken = default);

    ValueTask SetAsync(TenantRecord tenant, CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
