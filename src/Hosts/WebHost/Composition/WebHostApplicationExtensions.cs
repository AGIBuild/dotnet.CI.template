using Asp.Versioning;
using Asp.Versioning.Builder;
using ChengYuan.AspNetCore;
using ChengYuan.AspNetCore.Configuration;
using ChengYuan.Auditing;
using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;

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

        api.MapModuleEndpoints();

        api.MapGet("/app-configuration",
            static async (IApplicationConfigurator configurator, CancellationToken cancellationToken) =>
                TypedResults.Ok(await configurator.BuildAsync(cancellationToken)))
            .RequireAuthorization();

        return app;
    }
}
