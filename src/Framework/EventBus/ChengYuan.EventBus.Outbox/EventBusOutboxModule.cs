using System.Linq;
using ChengYuan.Core.Modularity;
using ChengYuan.Outbox;

namespace ChengYuan.EventBus.Outbox;

[DependsOn(typeof(EventBusModule))]
[DependsOn(typeof(OutboxModule))]
public sealed class EventBusOutboxModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddOutboxDistributedEventBus();
    }

    protected override IModuleDescriptor ResolveAttachedCapability(IModuleLoadContext context)
    {
        return Dependencies.Single(dependency => dependency.ModuleType == typeof(EventBusModule));
    }
}
