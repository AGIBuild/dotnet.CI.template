using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Auditing;

[DependsOn(typeof(ExecutionContextModule))]
[DependsOn(typeof(MultiTenancyModule))]
public sealed class AuditingModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddAuditing();
    }
}
