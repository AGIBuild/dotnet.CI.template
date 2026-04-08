using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Auditing;

public interface IAuditLogSink
{
    ValueTask WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}
