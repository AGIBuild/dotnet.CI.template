using ChengYuan.Core.Modularity;
using ChengYuan.Features;

namespace ChengYuan.FeatureManagement;

[DependsOn(typeof(FeaturesModule))]
public sealed class FeatureManagementModule : ApplicationModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddFeatureManagement();
    }
}
