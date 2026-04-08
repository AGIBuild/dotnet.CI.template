using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.ExecutionContext;

[DependsOn(typeof(global::ChengYuan.Core.CoreRuntimeModule))]
public sealed class ExecutionContextModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddExecutionContext();
    }
}
