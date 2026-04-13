using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ChengYuan.Auditing;

internal sealed class AuditMiddleware(RequestDelegate next, IOptions<AuditMiddlewareOptions> options)
{
    private readonly AuditMiddlewareOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext httpContext, IAuditScopeFactory auditScopeFactory)
    {
        if (_options.RequestFilter is not null && !_options.RequestFilter(httpContext))
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
}
