using System;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.Outbox;

namespace ChengYuan.EventBus.Outbox;

public sealed class EventBusOutboxDispatcher(
    ILocalEventBus localEventBus,
    IOutboxSerializer serializer) : IOutboxDispatcher
{
    public async ValueTask DispatchAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var eventType = Type.GetType(message.Payload.TypeName)
            ?? throw new InvalidOperationException($"Could not resolve type '{message.Payload.TypeName}' for outbox message '{message.Id}'.");

        var eventData = serializer.Deserialize(message.Payload, eventType)
            ?? throw new InvalidOperationException($"Failed to deserialize outbox message '{message.Id}' as type '{eventType.FullName}'.");

        await localEventBus.PublishAsync(eventType, eventData, cancellationToken);
    }
}
