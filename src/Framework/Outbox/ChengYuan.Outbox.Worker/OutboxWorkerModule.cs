using System.Linq;
using ChengYuan.BackgroundWorkers;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Outbox;

[DependsOn(typeof(OutboxModule))]
[DependsOn(typeof(BackgroundWorkersModule))]
public sealed class OutboxWorkerModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<IOutboxWorker, DefaultOutboxWorker>();
        context.Services.AddSingleton<IBackgroundWorker, OutboxDrainBackgroundWorker>();
    }

    protected override IModuleDescriptor ResolveAttachedCapability(IModuleLoadContext context)
    {
        return Dependencies.Single(dependency => dependency.ModuleType == typeof(OutboxModule));
    }
}
