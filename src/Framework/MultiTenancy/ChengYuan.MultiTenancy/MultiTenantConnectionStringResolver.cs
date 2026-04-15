using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Core.Data;
using Microsoft.Extensions.Options;

namespace ChengYuan.MultiTenancy;

/// <summary>
/// Connection string resolver that supports per-tenant overrides.
/// When a tenant is active, it checks for tenant-specific connection strings
/// before falling back to the default resolution from <see cref="ConnectionStringOptions"/>.
/// </summary>
public sealed class MultiTenantConnectionStringResolver(
    ICurrentTenant currentTenant,
    IOptions<ConnectionStringOptions> options) : IConnectionStringResolver
{
    public Task<string?> ResolveAsync(string? connectionStringName = null, CancellationToken cancellationToken = default)
    {
        if (currentTenant.Id is not null)
        {
            var tenantKey = $"{currentTenant.Id}:{connectionStringName ?? ConnectionStringOptions.DefaultConnectionStringName}";
            if (options.Value.ConnectionStrings.TryGetValue(tenantKey, out var tenantConnectionString))
            {
                return Task.FromResult<string?>(tenantConnectionString);
            }
        }

        // Fallback to standard named/default resolution
        if (connectionStringName is null)
        {
            return Task.FromResult(options.Value.Default);
        }

        return options.Value.ConnectionStrings.TryGetValue(connectionStringName, out var connectionString)
            ? Task.FromResult<string?>(connectionString)
            : Task.FromResult(options.Value.Default);
    }
}
