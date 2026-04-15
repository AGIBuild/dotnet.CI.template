using ChengYuan.Core.Data.Auditing;
using Microsoft.EntityFrameworkCore;

namespace ChengYuan.AuditLogging;

public sealed class AuditLogStore(IDbContextFactory<AuditLoggingDbContext> dbContextFactory) : IAuditLogStore
{
    public async ValueTask AppendAsync(AuditLogRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var auditLogId = Guid.NewGuid();
        var auditLogEntity = new AuditLogEntity(
            auditLogId,
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
            record.Properties);

        foreach (var entityChange in record.EntityChanges)
        {
            auditLogEntity.EntityChanges.Add(new AuditLogEntityChangeEntity(
                Guid.NewGuid(),
                auditLogId,
                entityChange.EntityTypeFullName,
                entityChange.EntityId,
                entityChange.ChangeType,
                entityChange.ChangeTime,
                entityChange.PropertyChanges));
        }

        await dbContext.AuditLogs.AddAsync(auditLogEntity, cancellationToken);
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
        entity.ReadProperties(),
        []);
}
