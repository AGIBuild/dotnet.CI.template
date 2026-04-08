using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChengYuan.Core.StronglyTypedIds;

namespace ChengYuan.Core.Json;

public class StronglyTypedIdJsonConverter<TStronglyTypedId, TValue> : JsonConverter<TStronglyTypedId>
    where TStronglyTypedId : StronglyTypedId<TValue>
    where TValue : notnull
{
    public override TStronglyTypedId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = JsonSerializer.Deserialize<TValue>(ref reader, options);

        if (value is null)
        {
            throw new JsonException($"Unable to deserialize '{typeof(TStronglyTypedId).FullName}' from a null value.");
        }

        return StronglyTypedIdActivator.Create<TStronglyTypedId, TValue>(value);
    }

    public override void Write(Utf8JsonWriter writer, TStronglyTypedId value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
