using ChengYuan.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.EventBus.Outbox;

public static class EventBusOutboxServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxDistributedEventBus(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Replace(ServiceDescriptor.Scoped<IDistributedEventBus, OutboxDistributedEventBus>());
        services.TryAddScoped<IOutboxDispatcher, EventBusOutboxDispatcher>();

        return services;
    }
}
