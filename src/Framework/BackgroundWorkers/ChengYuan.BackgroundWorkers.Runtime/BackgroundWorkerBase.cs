using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ChengYuan.BackgroundWorkers;

public abstract class BackgroundWorkerBase(ILogger logger) : IBackgroundWorker
{
    protected ILogger Logger { get; } = logger;

    public virtual Task StartAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
