using ChengYuan.Core.EntityFrameworkCore;
using ChengYuan.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.TenantManagement;

public static class TenantManagementPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddTenantManagementPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddEntityFrameworkCoreDataAccess<TenantManagementDbContext>();
        services.TryAddScoped<ITenantStore, EfTenantStore>();
        services.TryAddScoped<ITenantReader>(serviceProvider => serviceProvider.GetRequiredService<ITenantStore>());
        services.Replace(ServiceDescriptor.Scoped<ITenantResolutionStore>(serviceProvider =>
            new TenantResolutionStoreAdapter(serviceProvider.GetRequiredService<ITenantReader>())));

        return services;
    }

    public static IServiceCollection AddTenantManagementPersistenceDbContext(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDbContext);

        services.AddDbContext<TenantManagementDbContext>(configureDbContext);

        return services;
    }
}
