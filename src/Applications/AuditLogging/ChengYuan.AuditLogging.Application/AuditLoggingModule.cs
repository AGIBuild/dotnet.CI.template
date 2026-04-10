using ChengYuan.Auditing;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.AuditLogging;

[DependsOn(typeof(AuditingModule))]
public sealed class AuditLoggingModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddAuditLogging();
    }
}
