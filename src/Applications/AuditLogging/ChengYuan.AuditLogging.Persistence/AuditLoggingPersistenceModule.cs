using ChengYuan.Core.Modularity;

namespace ChengYuan.AuditLogging;

[DependsOn(typeof(AuditLoggingModule))]
public sealed class AuditLoggingPersistenceModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAuditLoggingPersistence();
    }
}
