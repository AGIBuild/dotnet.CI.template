using ChengYuan.Core.Data.Auditing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Auditing;

public static class AuditingServiceCollectionExtensions
{
    public static IServiceCollection AddAuditing(this IServiceCollection services)
    {
        services.TryAddSingleton<IAuditScopeAccessor, AmbientAuditScopeAccessor>();
        services.TryAddSingleton<IAuditScopeFactory, DefaultAuditScopeFactory>();
        services.TryAddSingleton<IEntityChangeCollector, AuditScopeEntityChangeCollector>();
        return services;
    }

    public static IServiceCollection AddInMemoryAuditing(this IServiceCollection services)
    {
        services.TryAddSingleton<InMemoryAuditLogCollector>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuditLogSink, InMemoryAuditLogSink>());
        return services;
    }
}
