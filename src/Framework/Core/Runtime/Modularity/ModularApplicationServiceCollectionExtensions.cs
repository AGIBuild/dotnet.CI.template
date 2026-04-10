using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Core.Modularity;

public static class ModularApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddModularApplication<TStartupModule>(
        this IServiceCollection services,
        Action<ModularApplicationOptions>? configure = null)
        where TStartupModule : ModuleBase, new()
    {
        services.AddModule<TStartupModule>();

        var options = new ModularApplicationOptions { StartupModuleType = typeof(TStartupModule) };
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.AddSingleton<IModularApplication>(serviceProvider =>
            new ModularApplication(
                serviceProvider,
                serviceProvider.GetRequiredService<IModuleCatalog>(),
                serviceProvider.GetRequiredService<IModuleManager>()));

        return services;
    }
}
