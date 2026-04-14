using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Outbox;

public interface IOutbox
{
    ValueTask<Guid> EnqueueAsync<T>(string name, T payload, CancellationToken cancellationToken = default);
}
