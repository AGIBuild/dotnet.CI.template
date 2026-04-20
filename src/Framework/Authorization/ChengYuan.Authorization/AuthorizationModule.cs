using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.Features;
using ChengYuan.MultiTenancy;

namespace ChengYuan.Authorization;

[DependsOn(typeof(ExecutionContextModule))]
[DependsOn(typeof(FeaturesModule))]
[DependsOn(typeof(MultiTenancyModule))]
public sealed class AuthorizationModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAuthorizationCore();
    }
}
