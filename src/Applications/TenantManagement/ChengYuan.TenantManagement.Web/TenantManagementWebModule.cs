using ChengYuan.Core.Modularity;

namespace ChengYuan.TenantManagement;

[DependsOn(typeof(TenantManagementPersistenceModule))]
public sealed class TenantManagementWebModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTenantManagementWeb();
    }
}
