using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Identity;

[DependsOn(typeof(IdentityModule))]
public sealed class IdentityPersistenceModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddIdentityPersistence();
    }
}
