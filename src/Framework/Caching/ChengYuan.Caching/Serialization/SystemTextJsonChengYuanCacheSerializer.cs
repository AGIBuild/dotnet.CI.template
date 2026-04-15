using System;
using System.Text.Json;
using ChengYuan.Core.Json;
using Microsoft.Extensions.Options;

namespace ChengYuan.Caching;

internal sealed class SystemTextJsonChengYuanCacheSerializer(IOptions<ChengYuanJsonOptions> jsonOptions) : IChengYuanCacheSerializer
{
    private readonly JsonSerializerOptions _serializerOptions = jsonOptions.Value.JsonSerializerOptions;

    public ChengYuanCacheItem Serialize<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var type = typeof(T);
        return new ChengYuanCacheItem(
            JsonSerializer.SerializeToUtf8Bytes(value, _serializerOptions),
            type.AssemblyQualifiedName ?? type.FullName ?? type.Name);
    }

    public T? Deserialize<T>(ChengYuanCacheItem item)
    {
        return JsonSerializer.Deserialize<T>(item.Payload, _serializerOptions);
    }
}
