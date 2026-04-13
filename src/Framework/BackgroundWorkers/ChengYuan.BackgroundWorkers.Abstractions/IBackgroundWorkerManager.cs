using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.BackgroundWorkers;

public interface IBackgroundWorkerManager
{
    Task AddAsync(IBackgroundWorker worker, CancellationToken cancellationToken = default);

    Task StartAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
}
