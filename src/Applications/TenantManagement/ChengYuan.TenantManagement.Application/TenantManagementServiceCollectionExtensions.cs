using ChengYuan.Authorization;
using ChengYuan.Features;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.TenantManagement;

public static class TenantManagementServiceCollectionExtensions
{
    public static IServiceCollection AddTenantManagement(this IServiceCollection services)
    {
        services.TryAddScoped<ITenantManager, TenantManager>();
        services.AddTransient<IPermissionDefinitionContributor, TenantManagementPermissionDefinitionContributor>();
        services.AddTransient<IFeatureDefinitionContributor, TenantManagementFeatureDefinitionContributor>();
        return services;
    }

    public static IServiceCollection AddInMemoryTenantManagement(this IServiceCollection services)
    {
        services.TryAddSingleton<InMemoryTenantStore>();
        services.TryAddSingleton<ITenantStore>(serviceProvider => serviceProvider.GetRequiredService<InMemoryTenantStore>());
        services.TryAddSingleton<ITenantReader>(serviceProvider => serviceProvider.GetRequiredService<InMemoryTenantStore>());
        services.Replace(ServiceDescriptor.Singleton<ITenantResolutionStore>(serviceProvider =>
            new TenantResolutionStoreAdapter(serviceProvider.GetRequiredService<ITenantReader>())));

        return services;
    }
}
