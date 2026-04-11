using Microsoft.AspNetCore.Builder;

namespace ChengYuan.WebHost;

public static class MultiTenancyApplicationBuilderExtensions
{
    /// <summary>
    /// Inserts the tenant resolution middleware into the HTTP request pipeline.
    /// Place after authentication and before endpoint execution.
    /// </summary>
    public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantResolutionMiddleware>();
    }
}
