using ChengYuan.Core.Modularity;

namespace ChengYuan.PermissionManagement;

[DependsOn(typeof(PermissionManagementModule))]
public sealed class PermissionManagementWebModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddPermissionManagementWeb();
    }
}
