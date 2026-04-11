using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.MultiTenancy;

/// <summary>
/// Default no-op tenant resolution store. Direct Guid-based resolution works without
/// any store; name-based resolution remains unresolved when no concrete store is registered.
/// </summary>
internal sealed class NoOpTenantResolutionStore : ITenantResolutionStore
{
    public Task<TenantResolutionRecord?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult<TenantResolutionRecord?>(null);

    public Task<TenantResolutionRecord?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
        => Task.FromResult<TenantResolutionRecord?>(null);
}
