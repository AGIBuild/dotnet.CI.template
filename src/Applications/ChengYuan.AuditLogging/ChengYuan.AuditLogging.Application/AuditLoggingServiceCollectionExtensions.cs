using ChengYuan.Auditing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.AuditLogging;

public static class AuditLoggingServiceCollectionExtensions
{
    public static IServiceCollection AddAuditLogging(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuditLogSink, AuditLogStoreSink>());
        return services;
    }

    public static IServiceCollection AddInMemoryAuditLogging(this IServiceCollection services)
    {
        services.TryAddSingleton<InMemoryAuditLogStore>();
        services.TryAddSingleton<IAuditLogStore>(serviceProvider => serviceProvider.GetRequiredService<InMemoryAuditLogStore>());
        services.TryAddSingleton<IAuditLogReader>(serviceProvider => serviceProvider.GetRequiredService<InMemoryAuditLogStore>());
        return services;
    }
}
