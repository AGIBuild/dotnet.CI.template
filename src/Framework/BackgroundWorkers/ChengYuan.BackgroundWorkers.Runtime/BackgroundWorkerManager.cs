using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.BackgroundWorkers;

public sealed class BackgroundWorkerManager : IBackgroundWorkerManager, IDisposable
{
    private readonly List<IBackgroundWorker> _workers = [];
    private bool _isRunning;
    private bool _isDisposed;

    public async Task AddAsync(IBackgroundWorker worker, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(worker);

        _workers.Add(worker);

        if (_isRunning)
        {
            await worker.StartAsync(cancellationToken);
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _isRunning = true;

        foreach (var worker in _workers)
        {
            await worker.StartAsync(cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _isRunning = false;

        foreach (var worker in _workers)
        {
            await worker.StopAsync(cancellationToken);
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        foreach (var worker in _workers)
        {
            if (worker is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _workers.Clear();
    }
}
