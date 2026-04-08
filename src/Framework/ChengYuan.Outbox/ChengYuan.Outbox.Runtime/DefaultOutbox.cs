using System;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Core.Timing;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;

namespace ChengYuan.Outbox;

internal sealed class DefaultOutbox(
    IOutboxStore store,
    IOutboxSerializer serializer,
    IClock clock,
    ICurrentTenant currentTenant,
    ICurrentCorrelation currentCorrelation) : IOutbox
{
    public async ValueTask<Guid> EnqueueAsync<T>(string name, T payload, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(payload);

        var messageId = Guid.NewGuid();
        var message = new OutboxMessage(
            messageId,
            name,
            serializer.Serialize(payload),
            clock.UtcNow,
            currentTenant.Id,
            currentCorrelation.CorrelationId,
            OutboxMessageStatus.Pending,
            0,
            null,
            null);

        await store.SaveAsync(message, cancellationToken);
        return messageId;
    }
}
