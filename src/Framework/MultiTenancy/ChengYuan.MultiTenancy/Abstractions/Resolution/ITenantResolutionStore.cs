using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.MultiTenancy;

/// <summary>
/// Normalizes tenant candidates (id or name) into verified tenant records.
/// Implemented by the application layer (e.g. TenantManagement) to bridge
/// runtime resolution with persistent tenant data.
/// <para>
/// A no-op default is registered by the framework so that direct Guid-based
/// resolution works without any store. Name-based resolution remains unresolved
/// when no store is registered.
/// </para>
/// </summary>
public interface ITenantResolutionStore
{
    /// <summary>
    /// Finds a tenant by id. Returns null if the tenant does not exist.
    /// </summary>
    Task<TenantResolutionRecord?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a tenant by name. Returns null if the tenant does not exist.
    /// </summary>
    Task<TenantResolutionRecord?> FindByNameAsync(string name, CancellationToken cancellationToken = default);
}
