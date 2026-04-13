using System;
using System.Threading.Tasks;
using ChengYuan.BackgroundWorkers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChengYuan.Outbox;

internal sealed partial class OutboxDrainBackgroundWorker : AsyncPeriodicBackgroundWorkerBase
{
    public OutboxDrainBackgroundWorker(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OutboxDrainBackgroundWorker> logger)
        : base(TimeSpan.FromSeconds(5), serviceScopeFactory, logger)
    {
    }

    protected override async Task DoWorkAsync(BackgroundWorkerContext workerContext)
    {
        var outboxWorker = workerContext.ServiceProvider.GetRequiredService<IOutboxWorker>();
        var result = await outboxWorker.DrainAsync(cancellationToken: workerContext.CancellationToken);

        if (result.AttemptedCount > 0)
        {
            LogDrainResult(Logger, result.DispatchedCount, result.FailedCount, result.AttemptedCount);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Outbox drained: {DispatchedCount} dispatched, {FailedCount} failed out of {AttemptedCount} attempted.")]
    private static partial void LogDrainResult(ILogger logger, int dispatchedCount, int failedCount, int attemptedCount);
}
