using Microsoft.AspNetCore.Builder;

namespace ChengYuan.Auditing;

public static class AuditMiddlewareExtensions
{
    public static IApplicationBuilder UseAuditing(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AuditMiddleware>();
    }
}
