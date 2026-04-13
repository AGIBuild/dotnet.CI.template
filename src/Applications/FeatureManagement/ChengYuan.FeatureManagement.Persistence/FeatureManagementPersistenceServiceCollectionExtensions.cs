using ChengYuan.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.FeatureManagement;

public static class FeatureManagementPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddFeatureManagementPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddPersistenceModule<FeatureManagementDbContext, IFeatureValueStore, IFeatureValueReader, FeatureValueStore>();

        return services;
    }
}
