using ChengYuan.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.SettingManagement;

public static class SettingManagementServiceCollectionExtensions
{
    public static IServiceCollection AddSettingManagement(this IServiceCollection services)
    {
        services.TryAddSingleton<ISettingValueManager, SettingValueManager>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISettingValueProvider, SettingStoreUserValueProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISettingValueProvider, SettingStoreTenantValueProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISettingValueProvider, SettingStoreGlobalValueProvider>());

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
