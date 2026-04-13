using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.WebHost;

public static class WebModularApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddWebModularApplication<TStartupModule>(
        this IServiceCollection services,
        Action<ModularApplicationOptions>? configure = null)
        where TStartupModule : ModuleBase, new()
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddModularApplication<TStartupModule>(configure);
        services.AddHostedService<ModularApplicationHostedService>();
        return services;
    }
}
