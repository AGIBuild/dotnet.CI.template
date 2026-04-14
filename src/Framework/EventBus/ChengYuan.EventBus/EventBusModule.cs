using ChengYuan.Core.Data;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.EventBus;

[DependsOn(typeof(DataModule))]
public sealed class EventBusModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddLocalEventBus();
        context.Services.AddScoped<IDomainEventPublisher, EventBusDomainEventPublisher>();
    }
}
