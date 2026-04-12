using ChengYuan.Core;
using ChengYuan.Core.Modularity;

namespace ChengYuan.Identity;

[DependsOn(typeof(CoreRuntimeModule))]
public sealed class IdentityModule : ApplicationModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddIdentityApplication();
    }
}
