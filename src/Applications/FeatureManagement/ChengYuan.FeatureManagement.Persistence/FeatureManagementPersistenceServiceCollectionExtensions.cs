using ChengYuan.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.FeatureManagement;

public static class FeatureManagementPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddFeatureManagementPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddConfiguredDbContext<FeatureManagementDbContext>();
        services.AddConfiguredDbContextFactory<FeatureManagementDbContext>();
        services.AddEntityFrameworkCoreDataAccess<FeatureManagementDbContext>();
        services.TryAddSingleton<IFeatureValueStore, EfFeatureValueStore>();
        services.TryAddSingleton<IFeatureValueReader>(serviceProvider => serviceProvider.GetRequiredService<IFeatureValueStore>());

        return services;
    }
}
