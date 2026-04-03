using ChengYuan.Core.Modularity;
using ChengYuan.Core.Timing;
using ChengYuan.ExecutionContext;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Outbox;

[DependsOn(typeof(OutboxPersistenceModule))]
public sealed class OutboxWorkerModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IOutboxWorker, DefaultOutboxWorker>();
    }
}

internal sealed class DefaultOutboxWorker(
    IOutboxStore store,
    IOutboxDispatcher dispatcher,
    IClock clock) : IOutboxWorker
{
    public async ValueTask<OutboxDrainResult> DrainAsync(int maxCount = 100, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCount);

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
