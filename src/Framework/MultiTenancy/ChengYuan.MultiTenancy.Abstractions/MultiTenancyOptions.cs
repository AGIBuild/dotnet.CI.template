using System;
using System.Collections.Generic;

namespace ChengYuan.MultiTenancy;

/// <summary>
/// Top-level multi-tenancy configuration. Populated by <see cref="MultiTenancyBuilder"/>.
/// Scoped to Web host capabilities in the current version.
/// </summary>
public sealed class MultiTenancyOptions
{
    /// <summary>
    /// Master switch. When false, tenant resolution is skipped entirely.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Shared tenant key used by query string, route, and compatibility header/cookie lookup.
    /// Defaults to <c>"__tenant"</c> for ABP-compatible conventions.
    /// </summary>
    public string TenantKey { get; set; } = "__tenant";

    /// <summary>
    /// Behavior when no source resolves a tenant. Defaults to <see cref="UnresolvedTenantBehavior.Continue"/>.
    /// </summary>
    public UnresolvedTenantBehavior UnresolvedBehavior { get; set; } = UnresolvedTenantBehavior.Continue;

    /// <summary>
    /// Fallback tenant id when no source resolves. Null means no fallback.
    /// </summary>
    public Guid? FallbackTenantId { get; set; }

    /// <summary>
    /// Fallback tenant name, used alongside <see cref="FallbackTenantId"/>.
    /// </summary>
    public string? FallbackTenantName { get; set; }

    /// <summary>
    /// Claim types to inspect when resolving tenant from an authenticated user.
    /// Checked in order; first match wins.
    /// </summary>
    public IList<string> ClaimTypes { get; } = ["tenant_id", "tenantId", "tenant"];

    /// <summary>
    /// Header name used for HTTP header-based resolution.
    /// </summary>
    public string HeaderName { get; set; } = "X-Tenant-Id";

    /// <summary>
    /// Query string parameter name used for URL query-based resolution.
    /// </summary>
    public string QueryStringKey { get; set; } = "__tenant";

    /// <summary>
    /// Route parameter name used for route-based resolution.
    /// </summary>
    public string RouteKey { get; set; } = "__tenant";

    /// <summary>
    /// Cookie name used for cookie-based resolution.
    /// </summary>
    public string CookieName { get; set; } = "__tenant";

    /// <summary>
    /// Ordered list of domain/subdomain patterns for host-based resolution.
    /// Each pattern should contain a <c>{0}</c> placeholder for the tenant segment.
    /// Example: <c>"{0}.myapp.com"</c>
    /// </summary>
    public IList<string> DomainPatterns { get; } = [];
}
