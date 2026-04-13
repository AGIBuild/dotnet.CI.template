using ChengYuan.Core.Modularity;

namespace ChengYuan.ExceptionHandling;

[DependsOn(typeof(global::ChengYuan.Core.CoreRuntimeModule))]
public sealed class ExceptionHandlingModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddExceptionHandling();
    }
}
