using ChengYuan.Core.Modularity;
using ChengYuan.Settings;

namespace ChengYuan.SettingManagement;

[DependsOn(typeof(SettingsModule))]
public sealed class SettingManagementModule : ApplicationModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSettingManagement();
    }
}
