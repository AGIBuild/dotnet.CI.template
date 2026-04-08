using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Settings;

[DependsOn(typeof(ExecutionContextModule))]
[DependsOn(typeof(MultiTenancyModule))]
public sealed class SettingsModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSettings();
    }
}
