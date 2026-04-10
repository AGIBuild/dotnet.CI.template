using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.MultiTenancy;

[DependsOn(typeof(ExecutionContextModule))]
public sealed class MultiTenancyModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddMultiTenancy();
    }
}
