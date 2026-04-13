using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChengYuan.Core.StronglyTypedIds;

namespace ChengYuan.Core.Json;

public sealed class StronglyTypedIdJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return TryGetStronglyTypedIdValueType(typeToConvert, out _);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (!TryGetStronglyTypedIdValueType(typeToConvert, out var valueType))
        {
            throw new InvalidOperationException($"Type '{typeToConvert.FullName}' is not a strongly typed id.");
        }

        var converterType = typeof(StronglyTypedIdJsonConverter<,>).MakeGenericType(typeToConvert, valueType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private static bool TryGetStronglyTypedIdValueType(Type type, out Type valueType)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(StronglyTypedId<>))
            {
                valueType = current.GetGenericArguments()[0];
                return true;
            }
        }

        valueType = null!;
        return false;
    }
}
