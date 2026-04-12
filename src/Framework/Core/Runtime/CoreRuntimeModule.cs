using ChengYuan.Core.Lifecycle;
using ChengYuan.Core.Modularity;

namespace ChengYuan.Core;

public sealed class CoreRuntimeModule : FrameworkCoreModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddCoreRuntime();
    }
}
