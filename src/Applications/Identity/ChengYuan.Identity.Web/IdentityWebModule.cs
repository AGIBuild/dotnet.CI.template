using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Identity;

[DependsOn(typeof(IdentityModule))]
public sealed class IdentityWebModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddIdentityWeb();
    }
}
