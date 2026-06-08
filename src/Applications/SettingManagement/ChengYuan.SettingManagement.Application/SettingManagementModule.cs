using ChengYuan.Core.Modularity;
using ChengYuan.Core.Data;
using ChengYuan.Settings;

namespace ChengYuan.SettingManagement;

[DependsOn(typeof(SettingsModule))]
[DependsOn(typeof(DataModule))]
public sealed class SettingManagementModule : ApplicationModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSettingManagement();
    }
}
