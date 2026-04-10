using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Outbox;

[DependsOn(typeof(OutboxModule))]
public sealed class OutboxPersistenceModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IOutboxStore, InMemoryOutboxStore>();
    }
}
