using ChengYuan.Core.Modularity;
using ChengYuan.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.SettingManagement;

[DependsOn(typeof(SettingsModule))]
public sealed class SettingManagementModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSettingManagement();
    }
}
