using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Auditing;

internal sealed class InMemoryAuditLogSink(InMemoryAuditLogCollector collector) : IAuditLogSink
{
    public ValueTask WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        collector.Add(entry);
        return ValueTask.CompletedTask;
    }
}
