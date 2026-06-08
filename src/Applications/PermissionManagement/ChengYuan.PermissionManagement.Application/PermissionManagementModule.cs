using ChengYuan.Authorization;
using ChengYuan.Core.Data;
using ChengYuan.Core.Modularity;

namespace ChengYuan.PermissionManagement;

[DependsOn(typeof(AuthorizationModule))]
[DependsOn(typeof(DataModule))]
public sealed class PermissionManagementModule : ApplicationModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddPermissionManagement();
    }
}
