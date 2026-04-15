using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChengYuan.Core.Json;

public sealed class ChengYuanJsonOptions
{
    public JsonSerializerOptions JsonSerializerOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        Converters = { new StronglyTypedIdJsonConverterFactory(), new JsonStringEnumConverter() },
    };
}
