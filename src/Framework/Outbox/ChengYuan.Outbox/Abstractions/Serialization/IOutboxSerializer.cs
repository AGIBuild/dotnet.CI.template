using System;

namespace ChengYuan.Outbox;

public interface IOutboxSerializer
{
    OutboxPayload Serialize<T>(T payload);

    T? Deserialize<T>(OutboxPayload payload);

    object? Deserialize(OutboxPayload payload, Type targetType);
}
