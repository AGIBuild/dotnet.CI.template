using System;
using System.Threading;

namespace ChengYuan.BackgroundWorkers;

public sealed class BackgroundWorkerContext(IServiceProvider serviceProvider, CancellationToken cancellationToken)
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public CancellationToken CancellationToken { get; } = cancellationToken;
}
