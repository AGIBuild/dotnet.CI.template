using ChengYuan.BackgroundWorkers;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Outbox;

[DependsOn(typeof(OutboxPersistenceModule))]
[DependsOn(typeof(BackgroundWorkersModule))]
public sealed class OutboxWorkerModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<IOutboxWorker, DefaultOutboxWorker>();
        context.Services.AddSingleton<IBackgroundWorker, OutboxDrainBackgroundWorker>();
    }
}
