using System;

namespace ChengYuan.ObjectMapping;

internal sealed class DefaultObjectMapper(IObjectMappingProvider provider) : IObjectMapper
{
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return provider.Map<TSource, TDestination>(source);
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        return provider.Map(source, destination);
    }
}
