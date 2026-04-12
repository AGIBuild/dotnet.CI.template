using ChengYuan.Core.Lifecycle;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Core.Localization;

[DependsOn(typeof(global::ChengYuan.Core.CoreRuntimeModule))]
public sealed class LocalizationModule : FrameworkCoreModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.TryAddSingleton<IErrorLocalizer, ErrorLocalizer>();
    }
}
