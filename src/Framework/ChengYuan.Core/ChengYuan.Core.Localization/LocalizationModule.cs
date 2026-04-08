using ChengYuan.Core.Lifecycle;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Core.Localization;

[DependsOn(typeof(global::ChengYuan.Core.CoreRuntimeModule))]
public sealed class LocalizationModule : ModuleBase, IPreConfigureServices
{
    public void PreConfigureServices(IServiceCollection services)
    {
        services.TryAddSingleton<IErrorLocalizer, ErrorLocalizer>();
    }
}
