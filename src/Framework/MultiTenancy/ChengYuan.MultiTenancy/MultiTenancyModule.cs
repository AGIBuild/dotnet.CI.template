using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;

namespace ChengYuan.MultiTenancy;

[DependsOn(typeof(ExecutionContextModule))]
public sealed class MultiTenancyModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMultiTenancyCore();
    }
}
