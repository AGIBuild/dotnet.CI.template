using System;

namespace ChengYuan.MultiTenancy;

/// <summary>
/// The immutable outcome of the tenant resolution pipeline.
/// </summary>
public sealed record TenantResolveResult(
    Guid? TenantId,
    string? TenantName,
    TenantResolveOutcome Outcome)
{
    /// <summary>
    /// A result indicating no tenant was resolved and no candidate was supplied.
    /// </summary>
    public static TenantResolveResult Unresolved { get; } = new(null, null, TenantResolveOutcome.Unresolved);

    /// <summary>
    /// Creates a resolved result for the given tenant.
    /// </summary>
    public static TenantResolveResult Resolved(Guid tenantId, string? tenantName = null)
        => new(tenantId, tenantName, TenantResolveOutcome.Resolved);

    /// <summary>
    /// Creates a not-found result when a candidate was supplied but no matching tenant exists.
    /// </summary>
    public static TenantResolveResult NotFound() => new(null, null, TenantResolveOutcome.NotFound);

    /// <summary>
    /// Creates an inactive result when a tenant was found but is disabled.
    /// </summary>
    public static TenantResolveResult InactiveTenant(Guid tenantId, string? tenantName = null)
        => new(tenantId, tenantName, TenantResolveOutcome.Inactive);
}
