using ChengYuan.MultiTenancy;

namespace ChengYuan.TenantManagement;

/// <summary>
/// Bridges tenant catalog reads into the multi-tenancy resolution pipeline.
/// </summary>
public sealed class TenantResolutionStoreAdapter(ITenantReader reader) : ITenantResolutionStore
{
    public async Task<TenantResolutionRecord?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await reader.FindByIdAsync(id, cancellationToken);
        return record is null ? null : Map(record);
    }

    public async Task<TenantResolutionRecord?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var record = await reader.FindByNameAsync(name, cancellationToken);
        return record is null ? null : Map(record);
    }

    private static TenantResolutionRecord Map(TenantRecord record)
        => new(record.Id, record.Name, record.IsActive);
}