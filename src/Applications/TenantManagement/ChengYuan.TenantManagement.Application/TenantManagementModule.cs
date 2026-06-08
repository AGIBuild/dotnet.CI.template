using ChengYuan.Core.Data;
using ChengYuan.Core.Modularity;
using ChengYuan.MultiTenancy;

namespace ChengYuan.TenantManagement;

[DependsOn(typeof(MultiTenancyModule))]
[DependsOn(typeof(DataModule))]
public sealed class TenantManagementModule : ApplicationModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTenantManagement();
    }
}
