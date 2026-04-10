using System;
using System.Text.Json;

namespace ChengYuan.Outbox;

internal sealed class SystemTextJsonOutboxSerializer : IOutboxSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public OutboxPayload Serialize<T>(T payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var type = typeof(T);
        return new OutboxPayload(
            JsonSerializer.SerializeToUtf8Bytes(payload, SerializerOptions),
            type.AssemblyQualifiedName ?? type.FullName ?? type.Name);
    }

    public T? Deserialize<T>(OutboxPayload payload)
    {
        return JsonSerializer.Deserialize<T>(payload.Content, SerializerOptions);
    }
}
