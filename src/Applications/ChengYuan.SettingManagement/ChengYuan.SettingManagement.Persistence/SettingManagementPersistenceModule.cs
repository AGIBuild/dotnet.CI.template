using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.SettingManagement;

[DependsOn(typeof(SettingManagementModule))]
public sealed class SettingManagementPersistenceModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSettingManagementPersistence();
    }
}
