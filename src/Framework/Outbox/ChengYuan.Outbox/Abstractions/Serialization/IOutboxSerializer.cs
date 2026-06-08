using System;

namespace ChengYuan.Outbox;

public interface IOutboxSerializer
{
    OutboxPayload Serialize<T>(T payload);

    OutboxPayload Serialize(object payload, Type payloadType);

    T? Deserialize<T>(OutboxPayload payload);

    object? Deserialize(OutboxPayload payload, Type targetType);
}
