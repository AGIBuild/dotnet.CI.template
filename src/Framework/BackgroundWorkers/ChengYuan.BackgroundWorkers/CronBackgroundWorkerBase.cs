using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChengYuan.BackgroundWorkers;

public abstract partial class CronBackgroundWorkerBase : BackgroundWorkerBase
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly CronExpression _cronExpression;
    private CancellationTokenSource? _stoppingCts;
    private Task? _executingTask;

    protected CronBackgroundWorkerBase(
        string cronExpression,
        IServiceScopeFactory serviceScopeFactory,
        ILogger logger)
        : base(logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cronExpression);

        _serviceScopeFactory = serviceScopeFactory;
        _cronExpression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);
    }

    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executingTask = RunScheduleLoopAsync(_stoppingCts.Token);

        return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_executingTask is null)
        {
            return;
        }

        _stoppingCts?.Cancel();
        await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
    }

    protected abstract Task DoWorkAsync(BackgroundWorkerContext workerContext);

    private async Task RunScheduleLoopAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var utcNow = DateTimeOffset.UtcNow;
            var nextOccurrence = _cronExpression.GetNextOccurrence(utcNow, TimeZoneInfo.Utc);

            if (nextOccurrence is null)
            {
                break;
            }

            var delay = nextOccurrence.Value - utcNow;

            if (delay > TimeSpan.Zero)
            {
                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }

            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                await DoWorkAsync(new BackgroundWorkerContext(scope.ServiceProvider, stoppingToken));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                LogCronWorkerError(Logger, GetType().FullName, ex);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Cron background worker '{WorkerType}' encountered an error.")]
    private static partial void LogCronWorkerError(ILogger logger, string? workerType, Exception exception);
}
