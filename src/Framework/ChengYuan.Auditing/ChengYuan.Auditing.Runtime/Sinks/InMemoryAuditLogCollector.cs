using System;
using System.Collections.Generic;

namespace ChengYuan.Auditing;

public sealed class InMemoryAuditLogCollector
{
    private readonly object _sync = new();
    private readonly List<AuditLogEntry> _entries = [];

    public IReadOnlyList<AuditLogEntry> Entries
    {
        get
        {
            lock (_sync)
            {
                return _entries.ToArray();
            }
        }
    }

    internal void Add(AuditLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        lock (_sync)
        {
            _entries.Add(entry);
        }
    }
}
