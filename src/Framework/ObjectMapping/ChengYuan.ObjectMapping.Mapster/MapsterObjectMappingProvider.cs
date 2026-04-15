using Mapster;

namespace ChengYuan.ObjectMapping.Mapster;

internal sealed class MapsterObjectMappingProvider(TypeAdapterConfig config) : IObjectMappingProvider
{
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        return source.Adapt<TDestination>(config)!;
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        return source.Adapt(destination, config);
    }
}
