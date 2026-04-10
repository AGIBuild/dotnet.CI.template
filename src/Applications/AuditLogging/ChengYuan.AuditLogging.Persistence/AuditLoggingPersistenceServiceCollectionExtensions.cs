using ChengYuan.Core.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.AuditLogging;

public static class AuditLoggingPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddAuditLoggingPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddEntityFrameworkCoreDataAccess<AuditLoggingDbContext>();
        services.TryAddSingleton<IAuditLogStore, EfAuditLogStore>();
        services.TryAddSingleton<IAuditLogReader>(serviceProvider => serviceProvider.GetRequiredService<IAuditLogStore>());

        return services;
    }

    public static IServiceCollection AddAuditLoggingPersistenceDbContext(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDbContext);

        services.AddDbContext<AuditLoggingDbContext>(configureDbContext);
        services.AddDbContextFactory<AuditLoggingDbContext>(configureDbContext);

        return services;
    }
}
