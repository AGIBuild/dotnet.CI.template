namespace ChengYuan.ObjectMapping;

public interface IObjectMappingProvider
{
    TDestination Map<TSource, TDestination>(TSource source);

    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
}
