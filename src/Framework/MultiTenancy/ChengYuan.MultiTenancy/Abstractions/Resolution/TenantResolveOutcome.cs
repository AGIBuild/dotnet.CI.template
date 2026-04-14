namespace ChengYuan.MultiTenancy;

/// <summary>
/// Describes the outcome of the tenant resolution pipeline.
/// </summary>
public enum TenantResolveOutcome
{
    /// <summary>
    /// No source or contributor provided a tenant candidate.
    /// </summary>
    Unresolved,

    /// <summary>
    /// A tenant was successfully resolved and is active.
    /// </summary>
    Resolved,

    /// <summary>
    /// A candidate value was provided but the tenant was not found in the store.
    /// </summary>
    NotFound,

    /// <summary>
    /// The tenant was found but is currently inactive / disabled.
    /// </summary>
    Inactive
}
