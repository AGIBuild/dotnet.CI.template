namespace ChengYuan.TenantManagement;

public interface ITenantStore : ITenantReader
{
    ValueTask SetAsync(TenantRecord tenant, CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
