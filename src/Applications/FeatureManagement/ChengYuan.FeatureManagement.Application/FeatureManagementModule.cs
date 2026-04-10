using ChengYuan.Core.Modularity;
using ChengYuan.Features;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.FeatureManagement;

[DependsOn(typeof(FeaturesModule))]
public sealed class FeatureManagementModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddFeatureManagement();
    }
}
