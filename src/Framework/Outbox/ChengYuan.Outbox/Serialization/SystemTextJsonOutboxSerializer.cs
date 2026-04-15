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

        var type = typeof(T);
        return new OutboxPayload(
            JsonSerializer.SerializeToUtf8Bytes(payload, _serializerOptions),
            type.AssemblyQualifiedName ?? type.FullName ?? type.Name);
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
