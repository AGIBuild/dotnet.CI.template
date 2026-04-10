using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.PermissionManagement;

[DependsOn(typeof(PermissionManagementModule))]
public sealed class PermissionManagementPersistenceModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionManagementPersistence();
    }
}
