using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.Identity;
using ChengYuan.MultiTenancy;
using Microsoft.AspNetCore.Builder;

namespace ChengYuan.WebHost;

public static class WebHostApplicationExtensions
{
    public static WebApplication MapWebHostEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapGet("/health", (ModuleCatalog catalog, ICurrentCorrelation correlation, ICurrentTenant currentTenant) =>
            Results.Ok(new
            {
                status = "ok",
                correlationId = correlation.CorrelationId,
                tenantId = currentTenant.Id,
                modules = catalog.ModuleTypes.Select(moduleType => moduleType.Name)
            }));

        app.MapIdentityManagementEndpoints();
        return app;
    }
}
