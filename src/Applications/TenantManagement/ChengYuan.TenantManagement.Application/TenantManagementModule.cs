using ChengYuan.Core.Modularity;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.TenantManagement;

[DependsOn(typeof(MultiTenancyModule))]
public sealed class TenantManagementModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTenantManagement();
    }
}
