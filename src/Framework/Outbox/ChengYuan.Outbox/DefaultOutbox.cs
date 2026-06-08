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

        return await EnqueueCoreAsync(name, payload, typeof(T), cancellationToken);
    }

    public async ValueTask<Guid> EnqueueAsync(string name, object payload, Type payloadType, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(payloadType);

        return await EnqueueCoreAsync(name, payload, payloadType, cancellationToken);
    }

    private async ValueTask<Guid> EnqueueCoreAsync(string name, object payload, Type payloadType, CancellationToken cancellationToken)
    {
        var messageId = Guid.NewGuid();
        var message = new OutboxMessage(
            messageId,
            name,
            serializer.Serialize(payload, payloadType),
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
