using ChengYuan.Core.Extensions;
using ChengYuan.Core.Security;
using ChengYuan.Core.SimpleStateChecking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Core;

public static class CoreRuntimeServiceCollectionExtensions
{
    public static IServiceCollection AddCoreRuntime(this IServiceCollection services)
    {
        services.TryAddSingleton<ExtraPropertyManager>();
        services.AddOptions<StringEncryptionOptions>();
        services.TryAddSingleton<IStringEncryptionService, AesStringEncryptionService>();
        services.TryAddTransient<ISimpleStateCheckerManager, SimpleStateCheckerManager>();
        return services;
    }
}
