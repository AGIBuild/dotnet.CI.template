using System;

namespace ChengYuan.ObjectMapping;

internal sealed class NotConfiguredObjectMappingProvider : IObjectMappingProvider
{
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        throw new InvalidOperationException(
            $"No object mapping provider has been configured. Register an IObjectMappingProvider implementation (e.g., AutoMapper or Mapperly) in DI.");
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        throw new InvalidOperationException(
            $"No object mapping provider has been configured. Register an IObjectMappingProvider implementation (e.g., AutoMapper or Mapperly) in DI.");
    }
}
