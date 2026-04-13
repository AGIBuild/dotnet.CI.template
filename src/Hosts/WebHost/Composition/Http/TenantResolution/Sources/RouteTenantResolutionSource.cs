using System;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace ChengYuan.WebHost;

/// <summary>
/// Resolves tenant from route values.
/// </summary>
internal sealed class RouteTenantResolutionSource(MultiTenancyOptions options) : ITenantResolutionSource
{
    public int Order => 400;

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

        if (httpContext.GetRouteValue(options.RouteKey) is not string routeValue
            || string.IsNullOrWhiteSpace(routeValue))
        {
            return ValueTask.CompletedTask;
        }

        if (Guid.TryParse(routeValue, out var tenantId))
        {
            context.TenantId = tenantId;
            context.HasResolved = true;
            context.SourceName = nameof(RouteTenantResolutionSource);
            return ValueTask.CompletedTask;
        }

        context.TenantName = routeValue;
        context.SourceName = nameof(RouteTenantResolutionSource);

        return ValueTask.CompletedTask;
    }
}
