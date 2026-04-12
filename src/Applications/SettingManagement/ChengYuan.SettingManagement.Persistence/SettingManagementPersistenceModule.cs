using ChengYuan.Core.Modularity;

namespace ChengYuan.SettingManagement;

[DependsOn(typeof(SettingManagementModule))]
public sealed class SettingManagementPersistenceModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSettingManagementPersistence();
    }
}
