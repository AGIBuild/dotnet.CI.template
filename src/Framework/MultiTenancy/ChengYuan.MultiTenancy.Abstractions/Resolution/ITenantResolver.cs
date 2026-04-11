using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.MultiTenancy;

/// <summary>
/// Orchestrates the tenant resolution pipeline by running registered contributors in order.
/// The resolver owns orchestration and ordering; contributors are passive.
/// </summary>
public interface ITenantResolver
{
    /// <summary>
    /// Runs the resolution pipeline and returns the result.
    /// Returns <see cref="TenantResolveResult.Unresolved"/> if no contributor resolves
    /// and no fallback is configured.
    /// </summary>
    Task<TenantResolveResult> ResolveAsync(CancellationToken cancellationToken = default);
}
