using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Outbox;

public interface IOutboxWorker
{
    ValueTask<OutboxDrainResult> DrainAsync(int maxCount = 100, CancellationToken cancellationToken = default);
}
