using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Auditing;

public interface IAuditLogContributor
{
    ValueTask ContributeAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}
