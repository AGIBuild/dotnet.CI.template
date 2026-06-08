using System;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Core;
using ChengYuan.Core.Data;
using ChengYuan.Core.Timing;

namespace ChengYuan.Outbox;

internal sealed class DefaultOutboxWorker(
    IOutboxStore store,
    IOutboxDispatcher dispatcher,
    IClock clock,
    IUnitOfWorkManager unitOfWorkManager) : IOutboxWorker
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
                await using var unitOfWork = BeginUnitOfWork();
                await store.MarkDispatchedAsync(message.Id, clock.UtcNow, cancellationToken);
                await unitOfWork.CompleteAsync(cancellationToken);
                dispatchedCount++;
            }
            catch (Exception exception)
            {
                await using var unitOfWork = BeginUnitOfWork();
                await store.MarkFailedAsync(message.Id, exception.Message, cancellationToken);
                await unitOfWork.CompleteAsync(cancellationToken);
                failedCount++;
            }
        }

        return new OutboxDrainResult(attemptedCount, dispatchedCount, failedCount);
    }

    private IUnitOfWork BeginUnitOfWork()
    {
        return unitOfWorkManager.Begin(new UnitOfWorkOptions
        {
            TransactionBehavior = UnitOfWorkTransactionBehavior.Enabled,
        });
    }
}
