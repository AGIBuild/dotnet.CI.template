using System;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace ChengYuan.WebHost;

/// <summary>
/// Resolves tenant from URL query string parameters.
/// </summary>
internal sealed class QueryStringTenantResolutionSource(MultiTenancyOptions options) : ITenantResolutionSource
{
    public int Order => 300;

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

        var value = httpContext.Request.Query[options.QueryStringKey].ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValueTask.CompletedTask;
        }

        if (Guid.TryParse(value, out var tenantId))
        {
            context.TenantId = tenantId;
            context.HasResolved = true;
            context.SourceName = nameof(QueryStringTenantResolutionSource);
            return ValueTask.CompletedTask;
        }

        context.TenantName = value;
        context.SourceName = nameof(QueryStringTenantResolutionSource);

        return ValueTask.CompletedTask;
    }
}
