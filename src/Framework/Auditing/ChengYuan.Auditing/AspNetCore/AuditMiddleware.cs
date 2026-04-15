using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ChengYuan.Auditing;

internal sealed class AuditMiddleware(
    RequestDelegate next,
    IOptions<AuditingOptions> auditingOptions,
    IOptions<AuditMiddlewareOptions> middlewareOptions)
{
    private readonly AuditingOptions _auditingOptions = auditingOptions.Value;
    private readonly AuditMiddlewareOptions _middlewareOptions = middlewareOptions.Value;

    public async Task InvokeAsync(HttpContext httpContext, IAuditScopeFactory auditScopeFactory)
    {
        if (!ShouldAudit(httpContext))
        {
            await next(httpContext);
            return;
        }

        var request = httpContext.Request;
        var scopeName = $"{request.Method} {request.Path}";

        await using var scope = auditScopeFactory.Create(scopeName);
        scope.SetProperty("httpMethod", request.Method);
        scope.SetProperty("url", request.Path.Value);
        scope.SetProperty("queryString", request.QueryString.Value);
        scope.SetProperty("clientIpAddress", httpContext.Connection.RemoteIpAddress?.ToString());

        try
        {
            await next(httpContext);

            if (httpContext.Response.StatusCode >= 400)
            {
                scope.MarkFailed($"HTTP {httpContext.Response.StatusCode}");
            }
            else
            {
                scope.MarkSucceeded();
            }
        }
        catch (Exception ex)
        {
            scope.MarkFailed(ex);
            throw;
        }

        scope.SetProperty("httpStatusCode", httpContext.Response.StatusCode);
    }

    private bool ShouldAudit(HttpContext httpContext)
    {
        if (!_auditingOptions.IsEnabled)
        {
            return false;
        }

        if (!_auditingOptions.IsEnabledForAnonymousUsers && !(httpContext.User.Identity?.IsAuthenticated ?? false))
        {
            return false;
        }

        if (!_auditingOptions.IsEnabledForGetRequests
            && string.Equals(httpContext.Request.Method, HttpMethods.Get, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (_middlewareOptions.RequestFilter is not null && !_middlewareOptions.RequestFilter(httpContext))
        {
            return false;
        }

        return true;
    }
}
