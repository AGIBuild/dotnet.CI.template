using ChengYuan.Core.Modularity;

namespace ChengYuan.TenantManagement;

[DependsOn(typeof(TenantManagementModule))]
public sealed class TenantManagementWebModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTenantManagementWeb();
    }
}
