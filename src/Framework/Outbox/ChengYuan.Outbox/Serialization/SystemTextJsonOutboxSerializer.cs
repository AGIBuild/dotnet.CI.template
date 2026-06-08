using System;
using System.Text.Json;
using ChengYuan.Core.Json;
using Microsoft.Extensions.Options;

namespace ChengYuan.Outbox;

internal sealed class SystemTextJsonOutboxSerializer(IOptions<ChengYuanJsonOptions> jsonOptions) : IOutboxSerializer
{
    private readonly JsonSerializerOptions _serializerOptions = jsonOptions.Value.JsonSerializerOptions;

    public OutboxPayload Serialize<T>(T payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return Serialize(payload, typeof(T));
    }

    public OutboxPayload Serialize(object payload, Type payloadType)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(payloadType);

        return new OutboxPayload(
            JsonSerializer.SerializeToUtf8Bytes(payload, payloadType, _serializerOptions),
            payloadType.AssemblyQualifiedName ?? payloadType.FullName ?? payloadType.Name);
    }

    public T? Deserialize<T>(OutboxPayload payload)
    {
        return JsonSerializer.Deserialize<T>(payload.Content, _serializerOptions);
    }

    public object? Deserialize(OutboxPayload payload, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(targetType);

        return JsonSerializer.Deserialize(payload.Content, targetType, _serializerOptions);
    }
}
