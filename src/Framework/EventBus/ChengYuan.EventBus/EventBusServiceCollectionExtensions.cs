using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.EventBus;

public static class EventBusServiceCollectionExtensions
{
    public static IServiceCollection AddLocalEventBus(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ILocalEventBus, LocalEventBus>();
        services.TryAddScoped<IDistributedEventBus, LocalDistributedEventBus>();
        services.TryAddScoped<DomainEventDispatcher>();

        return services;
    }

    public static IServiceCollection AddEventSubscriber<TEvent, TSubscriber>(this IServiceCollection services)
        where TEvent : class
        where TSubscriber : class, IEventSubscriber<TEvent>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(ServiceDescriptor.Transient<IEventSubscriber<TEvent>, TSubscriber>());

        return services;
    }
}
