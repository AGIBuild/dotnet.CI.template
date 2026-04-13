using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.DistributedLocking;

public interface IDistributedLock
{
    Task<IAsyncDisposable?> TryAcquireAsync(
        string name,
        TimeSpan timeout = default,
        CancellationToken cancellationToken = default);
}
