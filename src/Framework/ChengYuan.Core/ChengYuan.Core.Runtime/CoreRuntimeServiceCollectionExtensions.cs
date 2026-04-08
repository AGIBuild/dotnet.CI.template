using ChengYuan.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Core;

public static class CoreRuntimeServiceCollectionExtensions
{
    public static IServiceCollection AddCoreRuntime(this IServiceCollection services)
    {
        services.TryAddSingleton<ExtraPropertyManager>();
        return services;
    }
}
