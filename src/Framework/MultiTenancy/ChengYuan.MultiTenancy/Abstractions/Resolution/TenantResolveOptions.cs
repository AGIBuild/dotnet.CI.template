using System;
using System.Collections.Generic;

namespace ChengYuan.MultiTenancy;

/// <summary>
/// Configuration for the tenant resolution pipeline.
/// </summary>
public sealed class TenantResolveOptions
{
    /// <summary>
    /// Ordered list of contributor types. The resolver runs them in order;
    /// the first contributor that sets <see cref="TenantResolveContext.HasResolved"/> wins.
    /// </summary>
    public IList<Type> Contributors { get; } = [];

    /// <summary>
    /// The fallback tenant id to use when no contributor resolves a tenant.
    /// Null means unresolved scope — which is valid for single-scope mode.
    /// Fallback is a policy decision in options, not inside contributors.
    /// </summary>
    public Guid? FallbackTenantId { get; set; }

    /// <summary>
    /// The fallback tenant name, used alongside <see cref="FallbackTenantId"/>.
    /// </summary>
    public string? FallbackTenantName { get; set; }
}
