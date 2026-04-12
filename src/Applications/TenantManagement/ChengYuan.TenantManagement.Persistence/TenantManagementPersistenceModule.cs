using ChengYuan.Core.Modularity;

namespace ChengYuan.TenantManagement;

[DependsOn(typeof(TenantManagementModule))]
public sealed class TenantManagementPersistenceModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTenantManagementPersistence();
    }
}
