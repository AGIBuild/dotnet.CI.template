using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChengYuan.BackgroundWorkers;

public abstract partial class AsyncPeriodicBackgroundWorkerBase : BackgroundWorkerBase
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly TimeSpan _period;
    private CancellationTokenSource? _stoppingCts;
    private Task? _executingTask;

    protected AsyncPeriodicBackgroundWorkerBase(
        TimeSpan period,
        IServiceScopeFactory serviceScopeFactory,
        ILogger logger)
        : base(logger)
    {
        _period = period;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executingTask = ExecuteAsync(_stoppingCts.Token);

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

    private async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_period);

        while (!stoppingToken.IsCancellationRequested)
        {
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
                LogWorkerError(Logger, GetType().FullName, ex);
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    protected abstract Task DoWorkAsync(BackgroundWorkerContext workerContext);

    [LoggerMessage(Level = LogLevel.Error, Message = "Background worker '{WorkerType}' encountered an error.")]
    private static partial void LogWorkerError(ILogger logger, string? workerType, Exception exception);
}
