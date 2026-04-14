using System;
using System.Collections.Generic;

namespace ChengYuan.MultiTenancy;

/// <summary>
/// Carries the state through the tenant resolution pipeline.
/// Each contributor or source reads from and writes to this context.
/// </summary>
public sealed class TenantResolveContext
{
    /// <summary>
    /// The resolved tenant id, or null if not yet resolved.
    /// Once set by a contributor, subsequent contributors are skipped.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// The resolved tenant name, or null if not available.
    /// </summary>
    public string? TenantName { get; set; }

    /// <summary>
    /// Indicates whether a contributor has resolved the tenant.
    /// </summary>
    public bool HasResolved { get; set; }

    /// <summary>
    /// Indicates whether any source or contributor has supplied a candidate value
    /// (id or name), regardless of whether it was successfully verified.
    /// </summary>
    public bool HasCandidate => TenantId.HasValue || !string.IsNullOrEmpty(TenantName);

    /// <summary>
    /// The name of the source or contributor that provided the current candidate.
    /// Used for diagnostics and tracing.
    /// </summary>
    public string? SourceName { get; set; }

    /// <summary>
    /// Shared property bag for contributors to pass data forward.
    /// </summary>
    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();
}
