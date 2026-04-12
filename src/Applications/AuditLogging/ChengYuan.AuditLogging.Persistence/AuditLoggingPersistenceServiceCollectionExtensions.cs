using ChengYuan.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.AuditLogging;

public static class AuditLoggingPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddAuditLoggingPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddConfiguredDbContext<AuditLoggingDbContext>();
        services.AddConfiguredDbContextFactory<AuditLoggingDbContext>();
        services.AddEntityFrameworkCoreDataAccess<AuditLoggingDbContext>();
        services.TryAddSingleton<IAuditLogStore, EfAuditLogStore>();
        services.TryAddSingleton<IAuditLogReader>(serviceProvider => serviceProvider.GetRequiredService<IAuditLogStore>());

        return services;
    }
}
