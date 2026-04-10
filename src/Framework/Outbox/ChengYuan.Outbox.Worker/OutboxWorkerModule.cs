using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Outbox;

[DependsOn(typeof(OutboxPersistenceModule))]
public sealed class OutboxWorkerModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IOutboxWorker, DefaultOutboxWorker>();
    }
}
