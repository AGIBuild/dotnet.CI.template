using ChengYuan.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.AuditLogging;

public static class AuditLoggingPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddAuditLoggingPersistence(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddPersistenceModule<AuditLoggingDbContext, IAuditLogStore, IAuditLogReader, EfAuditLogStore>();

        return services;
    }
}
