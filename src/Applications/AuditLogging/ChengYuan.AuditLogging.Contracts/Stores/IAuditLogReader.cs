using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.AuditLogging;

public interface IAuditLogReader
{
    ValueTask<IReadOnlyList<AuditLogRecord>> GetListAsync(CancellationToken cancellationToken = default);
}
