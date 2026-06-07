using ChengYuan.Authorization;
using ChengYuan.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.FeatureManagement;

public static class FeatureManagementServiceCollectionExtensions
{
    public static IServiceCollection AddFeatureManagement(this IServiceCollection services)
    {
        services.TryAddScoped<IFeatureValueManager, FeatureValueManager>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IFeatureValueProvider, FeatureStoreUserValueProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IFeatureValueProvider, FeatureStoreTenantValueProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IFeatureValueProvider, FeatureStoreGlobalValueProvider>());
        services.AddTransient<IPermissionDefinitionContributor, FeatureManagementPermissionDefinitionContributor>();

        return services;
    }

    public static IServiceCollection AddInMemoryFeatureManagement(this IServiceCollection services)
    {
        services.TryAddSingleton<InMemoryFeatureValueStore>();
        services.TryAddSingleton<IFeatureValueStore>(serviceProvider => serviceProvider.GetRequiredService<InMemoryFeatureValueStore>());
        services.TryAddSingleton<IFeatureValueReader>(serviceProvider => serviceProvider.GetRequiredService<InMemoryFeatureValueStore>());

        return services;
    }
}
