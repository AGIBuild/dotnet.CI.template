using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.MultiTenancy;

/// <summary>
/// Fluent builder for configuring multi-tenancy behavior.
/// Used as the callback parameter of <c>AddMultiTenancy(...)</c>.
/// Scoped to Web host capabilities in the current version.
/// </summary>
public sealed class MultiTenancyBuilder
{
    internal MultiTenancyOptions Options { get; } = new() { IsEnabled = true };
    internal IServiceCollection Services { get; }

    /// <summary>
    /// Error handler delegate invoked by middleware when outcome is NotFound/Inactive/Unresolved+Fail.
    /// Return true if the response was handled; false to use default behavior.
    /// </summary>
    internal Func<HttpContext, TenantResolveResult, Task<bool>>? ErrorHandler { get; private set; }

    internal MultiTenancyBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Overrides the shared tenant key used by query string, route, and compatibility header/cookie lookup.
    /// </summary>
    public MultiTenancyBuilder UseTenantKey(string tenantKey)
    {
        Options.TenantKey = tenantKey ?? throw new ArgumentNullException(nameof(tenantKey));
        return this;
    }

    /// <summary>
    /// Configures claim types to inspect when resolving from an authenticated user.
    /// </summary>
    public MultiTenancyBuilder UseClaim(params string[] claimTypes)
    {
        ArgumentNullException.ThrowIfNull(claimTypes);
        Options.ClaimTypes.Clear();
        foreach (var claimType in claimTypes)
        {
            Options.ClaimTypes.Add(claimType);
        }

        return this;
    }

    /// <summary>
    /// Configures the HTTP header name for header-based resolution.
    /// </summary>
    public MultiTenancyBuilder UseHeader(string headerName = "X-Tenant-Id")
    {
        Options.HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
        return this;
    }

    /// <summary>
    /// Configures the query string parameter name for URL query-based resolution.
    /// </summary>
    public MultiTenancyBuilder UseQueryString(string parameterName = "__tenant")
    {
        Options.QueryStringKey = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
        return this;
    }

    /// <summary>
    /// Configures the route parameter name for route-based resolution.
    /// </summary>
    public MultiTenancyBuilder UseRoute(string routeKey = "__tenant")
    {
        Options.RouteKey = routeKey ?? throw new ArgumentNullException(nameof(routeKey));
        return this;
    }

    /// <summary>
    /// Configures the cookie name for cookie-based resolution.
    /// </summary>
    public MultiTenancyBuilder UseCookie(string cookieName = "__tenant")
    {
        Options.CookieName = cookieName ?? throw new ArgumentNullException(nameof(cookieName));
        return this;
    }

    /// <summary>
    /// Configures one or more ordered domain/subdomain patterns for host-based resolution.
    /// Each pattern should contain a <c>{0}</c> placeholder for the tenant segment.
    /// Example: <c>"{0}.myapp.com"</c>
    /// </summary>
    public MultiTenancyBuilder UseDomain(params string[] patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);
        Options.DomainPatterns.Clear();
        foreach (var pattern in patterns)
        {
            Options.DomainPatterns.Add(pattern);
        }

        return this;
    }

    /// <summary>
    /// Sets a fallback tenant used when no source provides a candidate.
    /// Fallback is NOT applied when a candidate was provided but not found or inactive.
    /// </summary>
    public MultiTenancyBuilder UseFallback(Guid tenantId, string? tenantName = null)
    {
        Options.FallbackTenantId = tenantId;
        Options.FallbackTenantName = tenantName;
        return this;
    }

    /// <summary>
    /// Requires a tenant to be resolved. Unresolved requests will fail.
    /// </summary>
    public MultiTenancyBuilder RequireResolvedTenant()
    {
        Options.UnresolvedBehavior = UnresolvedTenantBehavior.Fail;
        return this;
    }

    /// <summary>
    /// Configures a custom error handler invoked by middleware for non-resolved outcomes.
    /// Return true if the response was handled; false to use default behavior.
    /// </summary>
    public MultiTenancyBuilder ConfigureErrorHandler(
        Func<HttpContext, TenantResolveResult, Task<bool>> handler)
    {
        ErrorHandler = handler ?? throw new ArgumentNullException(nameof(handler));
        return this;
    }

    /// <summary>
    /// Registers a custom tenant resolve contributor. The contributor is added to the
    /// end of the resolution pipeline and registered in DI automatically.
    /// </summary>
    public MultiTenancyBuilder AddContributor<TContributor>()
        where TContributor : class, ITenantResolveContributor
    {
        Services.AddTransient<TContributor>();
        Services.Configure<TenantResolveOptions>(options =>
            options.Contributors.Add(typeof(TContributor)));
        return this;
    }

    /// <summary>
    /// Registers a custom tenant resolution source for host-specific input extraction.
    /// The source is registered as a singleton in DI and participates in the ordered pipeline.
    /// </summary>
    public MultiTenancyBuilder AddSource<TSource>()
        where TSource : class, ITenantResolutionSource
    {
        Services.AddSingleton<ITenantResolutionSource, TSource>();
        return this;
    }
}
