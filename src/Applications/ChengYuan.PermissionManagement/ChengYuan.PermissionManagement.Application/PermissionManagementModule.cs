using ChengYuan.Authorization;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.PermissionManagement;

[DependsOn(typeof(AuthorizationModule))]
public sealed class PermissionManagementModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionManagement();
    }
}
