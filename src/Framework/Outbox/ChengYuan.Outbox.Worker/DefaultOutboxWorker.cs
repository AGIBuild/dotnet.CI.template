using System;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Core;
using ChengYuan.Core.Data;
using ChengYuan.Core.Timing;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Outbox;

internal sealed class DefaultOutboxWorker(IServiceScopeFactory serviceScopeFactory) : IOutboxWorker
{
    public async ValueTask<OutboxDrainResult> DrainAsync(int maxCount = 100, CancellationToken cancellationToken = default)
    {
        Check.Positive(maxCount, nameof(maxCount));

        var pendingMessages = await GetPendingMessagesAsync(maxCount, cancellationToken);
        var attemptedCount = 0;
        var dispatchedCount = 0;
        var failedCount = 0;

        foreach (var message in pendingMessages)
        {
            attemptedCount++;

            try
            {
                await DispatchAsync(message, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                await MarkFailedAsync(message.Id, exception.Message, cancellationToken);
                failedCount++;
                continue;
            }

            await MarkDispatchedAsync(message.Id, cancellationToken);
            dispatchedCount++;
        }

        return new OutboxDrainResult(attemptedCount, dispatchedCount, failedCount);
    }

    private async ValueTask<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(int maxCount, CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
        return await store.GetPendingAsync(maxCount, cancellationToken);
    }

    private async ValueTask DispatchAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IOutboxDispatcher>();
        await dispatcher.DispatchAsync(message, cancellationToken);
    }

    private async ValueTask MarkDispatchedAsync(Guid messageId, CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
        var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

        await using var unitOfWork = BeginUnitOfWork(unitOfWorkManager);
        await store.MarkDispatchedAsync(messageId, clock.UtcNow, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    private async ValueTask MarkFailedAsync(Guid messageId, string errorMessage, CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
        var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

        await using var unitOfWork = BeginUnitOfWork(unitOfWorkManager);
        await store.MarkFailedAsync(messageId, errorMessage, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    private static IUnitOfWork BeginUnitOfWork(IUnitOfWorkManager unitOfWorkManager)
    {
        return unitOfWorkManager.Begin(new UnitOfWorkOptions
        {
            TransactionBehavior = UnitOfWorkTransactionBehavior.Enabled,
        });
    }
}
