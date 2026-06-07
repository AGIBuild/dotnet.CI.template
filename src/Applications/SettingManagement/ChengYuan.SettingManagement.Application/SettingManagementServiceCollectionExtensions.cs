using ChengYuan.Authorization;
using ChengYuan.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.SettingManagement;

public static class SettingManagementServiceCollectionExtensions
{
    public static IServiceCollection AddSettingManagement(this IServiceCollection services)
    {
        services.TryAddScoped<ISettingValueManager, SettingValueManager>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISettingValueProvider, SettingStoreUserValueProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISettingValueProvider, SettingStoreTenantValueProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISettingValueProvider, SettingStoreGlobalValueProvider>());
        services.AddTransient<IPermissionDefinitionContributor, SettingManagementPermissionDefinitionContributor>();

        return services;
    }

    public static IServiceCollection AddInMemorySettingManagement(this IServiceCollection services)
    {
        services.TryAddSingleton<InMemorySettingValueStore>();
        services.TryAddSingleton<ISettingValueStore>(serviceProvider => serviceProvider.GetRequiredService<InMemorySettingValueStore>());
        services.TryAddSingleton<ISettingValueReader>(serviceProvider => serviceProvider.GetRequiredService<InMemorySettingValueStore>());

        return services;
    }
}
