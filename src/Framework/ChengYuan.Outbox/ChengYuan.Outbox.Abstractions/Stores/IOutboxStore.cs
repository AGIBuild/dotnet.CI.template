using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Outbox;

public interface IOutboxStore
{
    ValueTask SaveAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    ValueTask<OutboxMessage?> GetAsync(Guid messageId, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxCount, CancellationToken cancellationToken = default);

    ValueTask MarkDispatchedAsync(Guid messageId, DateTimeOffset dispatchedAtUtc, CancellationToken cancellationToken = default);

    ValueTask MarkFailedAsync(Guid messageId, string errorMessage, CancellationToken cancellationToken = default);
}
