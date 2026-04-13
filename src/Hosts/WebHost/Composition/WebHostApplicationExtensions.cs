using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.Identity;
using ChengYuan.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace ChengYuan.WebHost;

public static class WebHostApplicationExtensions
{
    public static WebApplication UseWebHostComposition(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMultiTenancy();
        app.MapWebHostEndpoints();

        return app;
    }

    public static WebApplication MapWebHostEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
        });

        app.MapGet("/health", (IModuleCatalog catalog, ICurrentCorrelation correlation, ICurrentTenant currentTenant) =>
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
