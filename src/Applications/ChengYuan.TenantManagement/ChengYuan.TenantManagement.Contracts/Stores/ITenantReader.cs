namespace ChengYuan.TenantManagement;

public interface ITenantReader
{
    ValueTask<TenantRecord?> FindByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    ValueTask<TenantRecord?> FindByNameAsync(string name, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<TenantRecord>> GetListAsync(CancellationToken cancellationToken = default);
}
