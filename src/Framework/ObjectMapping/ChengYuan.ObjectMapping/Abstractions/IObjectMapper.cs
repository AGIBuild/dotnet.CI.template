namespace ChengYuan.ObjectMapping;

public interface IObjectMapper
{
    TDestination Map<TSource, TDestination>(TSource source);

    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
}
