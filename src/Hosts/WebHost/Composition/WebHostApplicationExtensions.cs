using Asp.Versioning;
using Asp.Versioning.Builder;
using ChengYuan.Auditing;
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

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<SecurityHeadersMiddleware>();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseCors();
        app.UseAuthentication();
        app.UseMultiTenancy();
        app.UseRateLimiter();
        app.UseMiddleware<CurrentUserMiddleware>();
        app.UseAuditing();
        app.UseAuthorization();

        app.MapWebHostEndpoints();

        return app;
    }

    public static WebApplication MapWebHostEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapOpenApi();

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

        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .Build();

        var api = app.MapGroup("/api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet)
            .RequireRateLimiting("per-tenant");

        api.MapIdentityManagementEndpoints();
        api.MapAuditLogEndpoints();
        api.MapPermissionManagementEndpoints();
        api.MapFeatureManagementEndpoints();
        api.MapTenantManagementEndpoints();
        api.MapSettingManagementEndpoints();
        return app;
    }
}
