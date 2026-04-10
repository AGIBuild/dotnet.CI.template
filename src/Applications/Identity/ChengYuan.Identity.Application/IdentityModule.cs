using ChengYuan.Core;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Identity;

[DependsOn(typeof(CoreRuntimeModule))]
public sealed class IdentityModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddIdentityApplication();
    }
}
