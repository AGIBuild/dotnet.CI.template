using Microsoft.EntityFrameworkCore;

namespace ChengYuan.AuditLogging;

public sealed class AuditLogStore(IDbContextFactory<AuditLoggingDbContext> dbContextFactory) : IAuditLogStore
{
    public async ValueTask AppendAsync(AuditLogRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.AuditLogs.AddAsync(
            new AuditLogEntity(
                Guid.NewGuid(),
                record.Name,
                record.StartedAtUtc,
                record.CompletedAtUtc,
                record.Duration,
                record.TenantId,
                record.UserId,
                record.UserName,
                record.IsAuthenticated,
                record.CorrelationId,
                record.Succeeded,
                record.ErrorMessage,
                record.Properties),
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask<IReadOnlyList<AuditLogRecord>> GetListAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entities = await dbContext.AuditLogs
            .OrderBy(auditLog => auditLog.StartedAtUtc)
            .ThenBy(auditLog => auditLog.Id)
            .ToArrayAsync(cancellationToken);

        return entities
            .Select(MapToRecord)
            .ToArray();
    }

    private static AuditLogRecord MapToRecord(AuditLogEntity entity) => new(
        entity.Name,
        entity.StartedAtUtc,
        entity.CompletedAtUtc,
        entity.Duration,
        entity.TenantId,
        entity.UserId,
        entity.UserName,
        entity.IsAuthenticated,
        entity.CorrelationId,
        entity.Succeeded,
        entity.ErrorMessage,
        entity.ReadProperties());
}
