using ChengYuan.Core.Modularity;
using ChengYuan.MultiTenancy;

namespace ChengYuan.TenantManagement;

[DependsOn(typeof(MultiTenancyModule))]
public sealed class TenantManagementModule : ApplicationModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTenantManagement();
    }
}
