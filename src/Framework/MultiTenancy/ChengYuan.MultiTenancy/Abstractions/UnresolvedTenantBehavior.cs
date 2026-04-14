namespace ChengYuan.MultiTenancy;

/// <summary>
/// Controls behavior when no tenant can be resolved from any source.
/// </summary>
public enum UnresolvedTenantBehavior
{
    /// <summary>
    /// Continue execution without a tenant context. Valid for single-scope or host-side operations.
    /// </summary>
    Continue,

    /// <summary>
    /// Fail the operation. Useful for strict multi-tenant SaaS applications.
    /// </summary>
    Fail
}
