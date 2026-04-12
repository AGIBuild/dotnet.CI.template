using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;

namespace ChengYuan.Features;

[DependsOn(typeof(ExecutionContextModule))]
[DependsOn(typeof(MultiTenancyModule))]
public sealed class FeaturesModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddFeatures();
    }
}
