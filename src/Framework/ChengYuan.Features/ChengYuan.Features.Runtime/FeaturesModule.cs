using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Features;

[DependsOn(typeof(ExecutionContextModule))]
[DependsOn(typeof(MultiTenancyModule))]
public sealed class FeaturesModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddFeatures();
    }
}
