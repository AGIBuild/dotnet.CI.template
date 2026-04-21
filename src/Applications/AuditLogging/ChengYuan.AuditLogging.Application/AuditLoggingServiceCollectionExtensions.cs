using ChengYuan.Auditing;
using ChengYuan.Authorization;
using ChengYuan.Features;
using ChengYuan.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.AuditLogging;

public static class AuditLoggingServiceCollectionExtensions
{
    public static IServiceCollection AddAuditLogging(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuditLogSink, AuditLogStoreSink>());
        services.AddTransient<IPermissionDefinitionContributor, AuditLoggingPermissionDefinitionContributor>();
        services.AddTransient<ISettingDefinitionContributor, AuditLoggingSettingDefinitionContributor>();
        services.AddTransient<IFeatureDefinitionContributor, AuditLoggingFeatureDefinitionContributor>();
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
