using ChengYuan.Core.Modularity;

namespace ChengYuan.ExecutionContext;

[DependsOn(typeof(global::ChengYuan.Core.CoreRuntimeModule))]
public sealed class ExecutionContextModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddExecutionContext();
    }
}
