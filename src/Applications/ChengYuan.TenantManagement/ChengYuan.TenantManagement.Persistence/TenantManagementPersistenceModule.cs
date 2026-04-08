using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.TenantManagement;

[DependsOn(typeof(TenantManagementModule))]
public sealed class TenantManagementPersistenceModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTenantManagementPersistence();
    }
}
