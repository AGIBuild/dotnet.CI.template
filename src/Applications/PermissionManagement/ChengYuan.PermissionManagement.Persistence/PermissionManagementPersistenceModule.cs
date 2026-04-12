using ChengYuan.Core.Modularity;

namespace ChengYuan.PermissionManagement;

[DependsOn(typeof(PermissionManagementModule))]
public sealed class PermissionManagementPersistenceModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddPermissionManagementPersistence();
    }
}
