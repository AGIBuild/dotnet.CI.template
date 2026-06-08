using ChengYuan.Core.Data;
using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;

namespace ChengYuan.Auditing;

[DependsOn(typeof(ExecutionContextModule))]
[DependsOn(typeof(MultiTenancyModule))]
[DependsOn(typeof(DataModule))]
public sealed class AuditingModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAuditing();
    }
}
