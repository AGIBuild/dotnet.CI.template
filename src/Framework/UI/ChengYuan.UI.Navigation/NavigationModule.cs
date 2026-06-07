using ChengYuan.Core;
using ChengYuan.Core.Modularity;

namespace ChengYuan.Core.UI.Navigation;

[DependsOn(typeof(CoreRuntimeModule))]
public sealed class NavigationModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddNavigation();
    }
}
