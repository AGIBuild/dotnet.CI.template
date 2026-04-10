using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Authorization;

[DependsOn(typeof(ExecutionContextModule))]
[DependsOn(typeof(MultiTenancyModule))]
public sealed class AuthorizationModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthorizationCore();
    }
}
