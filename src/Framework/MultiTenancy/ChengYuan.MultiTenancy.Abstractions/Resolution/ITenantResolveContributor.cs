using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.MultiTenancy;

/// <summary>
/// A single source for tenant resolution.
/// Each contributor resolves from exactly one source (e.g., HTTP header, claim, env var).
/// Contributors must be side-effect free and must not call other contributors.
/// </summary>
public interface ITenantResolveContributor
{
    /// <summary>
    /// The name of this contributor, used for diagnostics and ordering.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Attempts to resolve the tenant from this contributor's source.
    /// Set <see cref="TenantResolveContext.HasResolved"/> to true if successful.
    /// </summary>
    Task ResolveAsync(TenantResolveContext context, CancellationToken cancellationToken = default);
}
