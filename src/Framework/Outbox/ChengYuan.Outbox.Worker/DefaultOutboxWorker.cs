using System;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Core;
using ChengYuan.Core.Timing;

namespace ChengYuan.Outbox;

internal sealed class DefaultOutboxWorker(
    IOutboxStore store,
    IOutboxDispatcher dispatcher,
    IClock clock) : IOutboxWorker
{
    public async ValueTask<OutboxDrainResult> DrainAsync(int maxCount = 100, CancellationToken cancellationToken = default)
    {
        Check.Positive(maxCount, nameof(maxCount));

        var pendingMessages = await store.GetPendingAsync(maxCount, cancellationToken);
        var attemptedCount = 0;
        var dispatchedCount = 0;
        var failedCount = 0;

        foreach (var message in pendingMessages)
        {
            attemptedCount++;

            try
            {
                await dispatcher.DispatchAsync(message, cancellationToken);
                await store.MarkDispatchedAsync(message.Id, clock.UtcNow, cancellationToken);
                dispatchedCount++;
            }
            catch (Exception exception)
            {
                await store.MarkFailedAsync(message.Id, exception.Message, cancellationToken);
                failedCount++;
            }
        }

        return new OutboxDrainResult(attemptedCount, dispatchedCount, failedCount);
    }
}
