using System.Threading.Tasks;
using ChengYuan.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace ChengYuan.WebHost;

/// <summary>
/// ASP.NET Core middleware that resolves the current tenant for each HTTP request
/// and wraps downstream processing in a tenant scope.
/// Branches on <see cref="TenantResolveOutcome"/> for fine-grained error handling.
/// </summary>
internal sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext httpContext,
        MultiTenancyOptions options,
        ITenantResolver resolver,
        ICurrentTenantAccessor accessor)
    {
        if (!options.IsEnabled)
        {
            await next(httpContext);
            return;
        }

        var result = await resolver.ResolveAsync(httpContext.RequestAborted);

        switch (result.Outcome)
        {
            case TenantResolveOutcome.Resolved:
                using (accessor.Change(result.TenantId, result.TenantName))
                {
                    await next(httpContext);
                }

                return;

            case TenantResolveOutcome.Unresolved:
                if (options.UnresolvedBehavior == UnresolvedTenantBehavior.Fail)
                {
                    if (await TryCustomHandler(httpContext, result))
                    {
                        return;
                    }

                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await httpContext.Response.WriteAsync("Tenant could not be resolved.", httpContext.RequestAborted);
                    return;
                }

                await next(httpContext);
                return;

            case TenantResolveOutcome.NotFound:
                if (await TryCustomHandler(httpContext, result))
                {
                    return;
                }

                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await httpContext.Response.WriteAsync("Tenant not found.", httpContext.RequestAborted);
                return;

            case TenantResolveOutcome.Inactive:
                if (await TryCustomHandler(httpContext, result))
                {
                    return;
                }

                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await httpContext.Response.WriteAsync("Tenant is inactive.", httpContext.RequestAborted);
                return;
        }
    }

    private static async Task<bool> TryCustomHandler(HttpContext httpContext, TenantResolveResult result)
    {
        var holder = httpContext.RequestServices.GetService(typeof(TenantResolutionErrorHandlerHolder))
            as TenantResolutionErrorHandlerHolder;
        if (holder is not null)
        {
            return await holder.Handler(httpContext, result);
        }

        return false;
    }
}