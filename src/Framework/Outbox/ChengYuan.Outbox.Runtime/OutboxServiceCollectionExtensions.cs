using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Outbox;

public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxRuntime(this IServiceCollection services)
    {
        services.AddSingleton<IOutboxSerializer, SystemTextJsonOutboxSerializer>();
        services.AddSingleton<IOutbox, DefaultOutbox>();

        return services;
    }
}
