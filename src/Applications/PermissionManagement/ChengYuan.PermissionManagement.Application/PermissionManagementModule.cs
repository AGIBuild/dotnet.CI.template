using ChengYuan.Authorization;
using ChengYuan.Core.Modularity;

namespace ChengYuan.PermissionManagement;

[DependsOn(typeof(AuthorizationModule))]
public sealed class PermissionManagementModule : ApplicationModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddPermissionManagement();
    }
}
