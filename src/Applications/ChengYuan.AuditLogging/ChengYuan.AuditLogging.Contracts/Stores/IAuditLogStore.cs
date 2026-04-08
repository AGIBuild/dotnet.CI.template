using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.AuditLogging;

public interface IAuditLogStore : IAuditLogReader
{
    ValueTask AppendAsync(AuditLogRecord record, CancellationToken cancellationToken = default);
}
