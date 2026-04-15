using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Auditing;
using ChengYuan.Core.Data.Auditing;

namespace ChengYuan.AuditLogging;

internal sealed class AuditLogStoreSink(IAuditLogStore store) : IAuditLogSink
{
    public ValueTask WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var record = new AuditLogRecord(
            entry.Name,
            entry.StartedAtUtc,
            entry.CompletedAtUtc,
            entry.Duration,
            entry.TenantId,
            entry.UserId,
            entry.UserName,
            entry.IsAuthenticated,
            entry.CorrelationId,
            entry.Succeeded,
            entry.ErrorMessage,
            new Dictionary<string, object?>(entry.Properties, StringComparer.Ordinal),
            entry.EntityChanges.ToList<EntityChangeInfo>());

        return store.AppendAsync(record, cancellationToken);
    }
}
