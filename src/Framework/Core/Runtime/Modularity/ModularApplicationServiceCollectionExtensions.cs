using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Core.Modularity;

public static class ModularApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddModularApplication<TStartupModule>(
        this IServiceCollection services,
        Action<ModularApplicationOptions>? configure = null)
        where TStartupModule : ModuleBase, new()
    {
        var options = new ModularApplicationOptions { StartupModuleType = typeof(TStartupModule) };
        foreach (var additionalModuleType in GetAdditionalModuleTypes(services))
        {
            options.AddAdditionalModule(additionalModuleType);
        }

        configure?.Invoke(options);

        services.AddModule(typeof(TStartupModule), options.AdditionalModuleTypes);
        services.AddSingleton(options);

        services.AddSingleton<IModularApplication>(serviceProvider =>
            new ModularApplication(
                serviceProvider,
                serviceProvider.GetRequiredService<IModuleCatalog>(),
                serviceProvider.GetRequiredService<IModuleManager>()));

        return services;
    }

    public static IServiceCollection AddAdditionalModule<TModule>(this IServiceCollection services)
        where TModule : ModuleBase
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddAdditionalModule(typeof(TModule));
    }

    public static IServiceCollection AddAdditionalModule(this IServiceCollection services, Type moduleType)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(moduleType);

        if (!typeof(ModuleBase).IsAssignableFrom(moduleType))
        {
            throw new InvalidOperationException(
                $"Module type '{moduleType.FullName}' must inherit from {nameof(ModuleBase)}.");
        }

        services.AddSingleton(new AdditionalModuleRegistration(moduleType));

        return services;
    }

    private static Type[] GetAdditionalModuleTypes(IServiceCollection services)
    {
        return services
            .Where(static descriptor => descriptor.ServiceType == typeof(AdditionalModuleRegistration))
            .Select(static descriptor => descriptor.ImplementationInstance)
            .OfType<AdditionalModuleRegistration>()
            .Select(static registration => registration.ModuleType)
            .Distinct()
            .ToArray();
    }
}
