using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace ChengYuan.WebHost;

/// <summary>
/// Resolves tenant from HTTP request headers.
/// Checks both the configured header name and the shared tenant key.
/// </summary>
internal sealed class HeaderTenantResolutionSource(MultiTenancyOptions options) : ITenantResolutionSource
{
    public int Order => 200;

    public bool IsAvailable(IServiceProvider serviceProvider)
    {
        var accessor = serviceProvider.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
        return accessor?.HttpContext is not null;
    }

    public ValueTask PopulateAsync(
        TenantResolveContext context,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        if (context.HasResolved)
        {
            return ValueTask.CompletedTask;
        }

        var accessor = serviceProvider.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
        var httpContext = accessor?.HttpContext;
        if (httpContext is null)
        {
            return ValueTask.CompletedTask;
        }

        if (TryResolveFromHeaderValue(httpContext, options.HeaderName, context))
        {
            return ValueTask.CompletedTask;
        }

        if (!string.Equals(options.HeaderName, options.TenantKey, StringComparison.OrdinalIgnoreCase))
        {
            TryResolveFromHeaderValue(httpContext, options.TenantKey, context);
        }

        return ValueTask.CompletedTask;
    }

    private bool TryResolveFromHeaderValue(HttpContext httpContext, string headerName, TenantResolveContext context)
    {
        if (!httpContext.Request.Headers.TryGetValue(headerName, out var headerValues))
        {
            return false;
        }

        var value = headerValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (Guid.TryParse(value, out var tenantId))
        {
            context.TenantId = tenantId;
            context.HasResolved = true;
            context.SourceName = nameof(HeaderTenantResolutionSource);
            return true;
        }

        context.TenantName = value;
        context.SourceName = nameof(HeaderTenantResolutionSource);
        return true;
    }
}
