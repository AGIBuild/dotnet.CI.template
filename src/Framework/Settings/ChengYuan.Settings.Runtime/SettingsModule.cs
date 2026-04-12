using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;

namespace ChengYuan.Settings;

[DependsOn(typeof(ExecutionContextModule))]
[DependsOn(typeof(MultiTenancyModule))]
public sealed class SettingsModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSettings();
    }
}
