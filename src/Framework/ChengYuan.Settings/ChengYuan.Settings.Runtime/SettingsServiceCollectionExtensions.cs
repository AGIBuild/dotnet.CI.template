using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Settings;

public static class SettingsServiceCollectionExtensions
{
    public static IServiceCollection AddSettings(this IServiceCollection services)
    {
        services.TryAddSingleton<ISettingDefinitionManager, DefaultSettingDefinitionManager>();
        services.TryAddSingleton<ISettingProvider, DefaultSettingProvider>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISettingValueProvider, UserSettingValueProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISettingValueProvider, TenantSettingValueProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISettingValueProvider, GlobalSettingValueProvider>());

        return services;
    }

    public static IServiceCollection AddInMemorySettings(this IServiceCollection services, Action<InMemorySettingsBuilder>? configure = null)
    {
        var builder = new InMemorySettingsBuilder();
        configure?.Invoke(builder);

        services.AddSingleton<ISettingValueProvider>(new InMemoryGlobalSettingValueProvider(builder.GlobalValues));
        services.AddSingleton<ISettingValueProvider>(new InMemoryTenantSettingValueProvider(builder.TenantValues));
        services.AddSingleton<ISettingValueProvider>(new InMemoryUserSettingValueProvider(builder.UserValues));

        return services;
    }
}
