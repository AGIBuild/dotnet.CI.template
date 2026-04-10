using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.AuditLogging;

public sealed class InMemoryAuditLogStore : IAuditLogStore
{
    private readonly object _sync = new();
    private readonly List<AuditLogRecord> _records = [];

    public ValueTask AppendAsync(AuditLogRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);

        lock (_sync)
        {
            _records.Add(record);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask<IReadOnlyList<AuditLogRecord>> GetListAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            return ValueTask.FromResult<IReadOnlyList<AuditLogRecord>>(_records.ToArray());
        }
    }
}
