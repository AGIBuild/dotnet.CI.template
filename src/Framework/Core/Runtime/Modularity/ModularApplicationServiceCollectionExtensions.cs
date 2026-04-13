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
        var options = BuildOptions<TStartupModule>(services, configure);

        services.AddModule(typeof(TStartupModule), options.AdditionalModuleTypes);
        RegisterModularApplication(services, options);

        return services;
    }

    public static async Task<IServiceCollection> AddModularApplicationAsync<TStartupModule>(
        this IServiceCollection services,
        Action<ModularApplicationOptions>? configure = null)
        where TStartupModule : ModuleBase, new()
    {
        var options = BuildOptions<TStartupModule>(services, configure);

        await services.AddModuleAsync(typeof(TStartupModule), options.AdditionalModuleTypes);
        RegisterModularApplication(services, options);

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

    private static ModularApplicationOptions BuildOptions<TStartupModule>(
        IServiceCollection services,
        Action<ModularApplicationOptions>? configure)
        where TStartupModule : ModuleBase
    {
        var options = new ModularApplicationOptions { StartupModuleType = typeof(TStartupModule) };
        foreach (var additionalModuleType in GetAdditionalModuleTypes(services))
        {
            options.AddAdditionalModule(additionalModuleType);
        }

        configure?.Invoke(options);
        return options;
    }

    private static void RegisterModularApplication(IServiceCollection services, ModularApplicationOptions options)
    {
        services.AddSingleton(options);

        services.AddSingleton<IModularApplication>(serviceProvider =>
            new ModularApplication(
                serviceProvider,
                serviceProvider.GetRequiredService<IModuleCatalog>(),
                serviceProvider.GetRequiredService<IModuleManager>()));
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
