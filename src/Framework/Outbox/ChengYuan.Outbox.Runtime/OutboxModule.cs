using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Outbox;

[DependsOn(typeof(ExecutionContextModule))]
[DependsOn(typeof(MultiTenancyModule))]
public sealed class OutboxModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddOutboxRuntime();
    }
}
