using ChengYuan.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.PermissionManagement;

public static class PermissionManagementServiceCollectionExtensions
{
    public static IServiceCollection AddPermissionManagement(this IServiceCollection services)
    {
        services.TryAddScoped<IPermissionGrantManager, PermissionGrantManager>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPermissionGrantProvider, PermissionStoreUserGrantProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPermissionGrantProvider, PermissionStoreTenantGrantProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPermissionGrantProvider, PermissionStoreGlobalGrantProvider>());
        services.AddTransient<IPermissionDefinitionContributor, PermissionManagementPermissionDefinitionContributor>();

        return services;
    }

    public static IServiceCollection AddInMemoryPermissionManagement(this IServiceCollection services)
    {
        services.TryAddSingleton<InMemoryPermissionGrantStore>();
        services.TryAddSingleton<IPermissionGrantStore>(serviceProvider => serviceProvider.GetRequiredService<InMemoryPermissionGrantStore>());
        services.TryAddSingleton<IPermissionGrantReader>(serviceProvider => serviceProvider.GetRequiredService<InMemoryPermissionGrantStore>());

        return services;
    }
}
