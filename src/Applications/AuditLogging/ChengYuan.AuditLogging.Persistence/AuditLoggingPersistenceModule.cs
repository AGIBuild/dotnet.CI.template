using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.AuditLogging;

[DependsOn(typeof(AuditLoggingModule))]
public sealed class AuditLoggingPersistenceModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddAuditLoggingPersistence();
    }
}
