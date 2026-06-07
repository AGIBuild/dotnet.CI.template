using ChengYuan.MultiTenancy;

namespace ChengYuan.FrameworkKernel.Tests;

internal sealed class InMemoryTenantResolutionStore(
    params TenantResolutionRecord[] records) : ITenantResolutionStore
{
    public Task<TenantResolutionRecord?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(records.FirstOrDefault(record => record.Id == id));

    public Task<TenantResolutionRecord?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
        => Task.FromResult(records.FirstOrDefault(record =>
            string.Equals(record.Name, name, StringComparison.OrdinalIgnoreCase)));
}
