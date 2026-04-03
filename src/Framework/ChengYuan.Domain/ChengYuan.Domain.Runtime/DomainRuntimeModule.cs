using ChengYuan.Core.Json;
using ChengYuan.Core.Modularity;

namespace ChengYuan.Domain;

[DependsOn(typeof(JsonModule))]
public sealed class DomainModule : ModuleBase
{
}

public class StronglyTypedIdJsonConverterFactory : ChengYuan.Core.Json.StronglyTypedIdJsonConverterFactory
{
}

public class StronglyTypedIdJsonConverter<TStronglyTypedId, TValue> : ChengYuan.Core.Json.StronglyTypedIdJsonConverter<TStronglyTypedId, TValue>
    where TStronglyTypedId : ChengYuan.Core.StronglyTypedIds.StronglyTypedId<TValue>
    where TValue : notnull
{
}
