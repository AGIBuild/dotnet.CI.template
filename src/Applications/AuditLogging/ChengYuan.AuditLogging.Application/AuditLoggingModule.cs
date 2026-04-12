using ChengYuan.Auditing;
using ChengYuan.Core.Modularity;

namespace ChengYuan.AuditLogging;

[DependsOn(typeof(AuditingModule))]
public sealed class AuditLoggingModule : ApplicationModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAuditLogging();
    }
}
