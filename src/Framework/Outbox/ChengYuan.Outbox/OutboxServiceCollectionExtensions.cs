using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Outbox;

public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxRuntime(this IServiceCollection services)
    {
        services.AddSingleton<IOutboxSerializer, SystemTextJsonOutboxSerializer>();
        services.TryAddSingleton<IOutboxStore, InMemoryOutboxStore>();
        services.AddScoped<IOutbox, DefaultOutbox>();

        return services;
    }
}
