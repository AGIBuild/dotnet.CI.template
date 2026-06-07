using ChengYuan.Core.Modularity;

namespace ChengYuan.AuditLogging;

[DependsOn(typeof(AuditLoggingModule))]
public sealed class AuditLoggingWebModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAuditLoggingWeb();
    }
}
