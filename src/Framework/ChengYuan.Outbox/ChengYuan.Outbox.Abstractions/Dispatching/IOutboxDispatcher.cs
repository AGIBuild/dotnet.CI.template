using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Outbox;

public interface IOutboxDispatcher
{
    ValueTask DispatchAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}
