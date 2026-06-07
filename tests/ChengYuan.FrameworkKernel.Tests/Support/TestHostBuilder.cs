using ChengYuan.AspNetCore;
using ChengYuan.AuditLogging;
using ChengYuan.BackgroundJobs;
using ChengYuan.FeatureManagement;
using ChengYuan.Hosting;
using ChengYuan.Identity;
using ChengYuan.PermissionManagement;
using ChengYuan.SettingManagement;
using ChengYuan.TenantManagement;
using ChengYuan.WebHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.FrameworkKernel.Tests;

internal static class TestHostBuilder
{
    internal const string TestJwtSecretKey = "TestSecretKeyThatIsAtLeast32CharactersLongForTesting!!";

    /// <summary>
    /// Registers the standard Web host composition used by integration tests.
    /// Equivalent to what WebHost/Program.cs does, but without a real DB provider
    /// (tests supply their own via <c>UseDbContextOptions</c>).
    /// </summary>
    public static WebApplicationBuilder AddTestWebHost(this WebApplicationBuilder builder)
    {
        builder.Configuration["Jwt:SecretKey"] = TestJwtSecretKey;
        builder.Configuration["Jwt:Issuer"] = "test";
        builder.Configuration["Jwt:Audience"] = "test";
        builder.Configuration["WebHost:Health:Detailed"] = "true";

        builder.AddChengYuan<WebHostHttpCompositionModule>(_ => { });
        builder.Services.AddPermissivePermissionGrants();
        return builder;
    }

    /// <summary>
    /// Registers the standard Web host composition on a raw <see cref="IServiceCollection"/>
    /// (for tests that don't need a full <see cref="WebApplicationBuilder"/>).
    /// </summary>
    public static IServiceCollection AddTestWebHost(this IServiceCollection services)
    {
        services.AddChengYuan<WebHostHttpCompositionModule>(_ => { });
        services.AddPermissivePermissionGrants();
        return services;
    }

    /// <summary>
    /// WebApplicationBuilder overload that supports configuring multi-tenancy.
    /// </summary>
    public static WebApplicationBuilder AddTestWebHost(
        this WebApplicationBuilder builder,
        Action<MultiTenancy.MultiTenancyBuilder>? configureMultiTenancy)
    {
        builder.Configuration["Jwt:SecretKey"] = TestJwtSecretKey;
        builder.Configuration["Jwt:Issuer"] = "test";
        builder.Configuration["Jwt:Audience"] = "test";
        builder.Configuration["WebHost:Health:Detailed"] = "true";

        builder.AddChengYuan<WebHostHttpCompositionModule>(cy =>
        {
            if (configureMultiTenancy is not null)
            {
                cy.ConfigureMultiTenancy(configureMultiTenancy);
            }

        });
        builder.Services.AddPermissivePermissionGrants();
        return builder;
    }
}
