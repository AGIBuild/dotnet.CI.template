using ChengYuan.AuditLogging;
using ChengYuan.Core.Modularity;
using ChengYuan.FeatureManagement;
using ChengYuan.Identity;
using ChengYuan.MultiTenancy;
using ChengYuan.PermissionManagement;
using ChengYuan.SettingManagement;
using ChengYuan.TenantManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.WebHost;

public static class WebHostServiceCollectionExtensions
{
    private const string DatabaseName = "chengyuan-web-host";

    public static IServiceCollection AddWebHostComposition(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? configureDbContext = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var effectiveConfigureDbContext = configureDbContext ?? CreateDefaultDbContextConfiguration();

        services.AddTenantManagementPersistenceDbContext(effectiveConfigureDbContext);
        services.AddSettingManagementPersistenceDbContext(effectiveConfigureDbContext);
        services.AddPermissionManagementPersistenceDbContext(effectiveConfigureDbContext);
        services.AddFeatureManagementPersistenceDbContext(effectiveConfigureDbContext);
        services.AddAuditLoggingPersistenceDbContext(effectiveConfigureDbContext);
        services.AddIdentityDbContext(effectiveConfigureDbContext);
        services.AddWebModularApplication<WebHostModule>();

        // Auto-wire Web tenant resolution sources in fixed priority order
        services.AddHttpContextAccessor();
        services.AddSingleton<ITenantResolutionSource>(sp =>
            new ClaimTenantResolutionSource(sp.GetRequiredService<MultiTenancyOptions>()));
        services.AddSingleton<ITenantResolutionSource>(sp =>
            new HeaderTenantResolutionSource(sp.GetRequiredService<MultiTenancyOptions>()));
        services.AddSingleton<ITenantResolutionSource>(sp =>
            new QueryStringTenantResolutionSource(sp.GetRequiredService<MultiTenancyOptions>()));
        services.AddSingleton<ITenantResolutionSource>(sp =>
            new RouteTenantResolutionSource(sp.GetRequiredService<MultiTenancyOptions>()));
        services.AddSingleton<ITenantResolutionSource>(sp =>
            new CookieTenantResolutionSource(sp.GetRequiredService<MultiTenancyOptions>()));
        services.AddSingleton<ITenantResolutionSource>(sp =>
            new DomainTenantResolutionSource(sp.GetRequiredService<MultiTenancyOptions>()));

        return services;
    }

    private static Action<DbContextOptionsBuilder> CreateDefaultDbContextConfiguration()
    {
        return options => options.UseInMemoryDatabase(DatabaseName);
    }
}
