using ChengYuan.Core.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.FeatureManagement;

public static class FeatureManagementPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddFeatureManagementPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddEntityFrameworkCoreDataAccess<FeatureManagementDbContext>();
        services.TryAddSingleton<IFeatureValueStore, EfFeatureValueStore>();
        services.TryAddSingleton<IFeatureValueReader>(serviceProvider => serviceProvider.GetRequiredService<IFeatureValueStore>());

        return services;
    }

    public static IServiceCollection AddFeatureManagementPersistenceDbContext(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDbContext);

        services.AddDbContext<FeatureManagementDbContext>(configureDbContext);
        services.AddDbContextFactory<FeatureManagementDbContext>(configureDbContext);

        return services;
    }
}
