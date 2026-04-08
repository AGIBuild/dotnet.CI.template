using System;
using System.Text.Json;

namespace ChengYuan.Caching;

internal sealed class SystemTextJsonChengYuanCacheSerializer : IChengYuanCacheSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public ChengYuanCacheItem Serialize<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var type = typeof(T);
        return new ChengYuanCacheItem(
            JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions),
            type.AssemblyQualifiedName ?? type.FullName ?? type.Name);
    }

    public T? Deserialize<T>(ChengYuanCacheItem item)
    {
        return JsonSerializer.Deserialize<T>(item.Payload, SerializerOptions);
    }
}
