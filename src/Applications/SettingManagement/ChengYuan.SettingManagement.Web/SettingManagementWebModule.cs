using ChengYuan.Core.Modularity;

namespace ChengYuan.SettingManagement;

[DependsOn(typeof(SettingManagementModule))]
public sealed class SettingManagementWebModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSettingManagementWeb();
    }
}
