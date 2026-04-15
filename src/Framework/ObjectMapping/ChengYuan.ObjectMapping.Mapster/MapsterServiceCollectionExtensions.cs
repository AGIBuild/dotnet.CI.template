using System;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.ObjectMapping.Mapster;

public static class MapsterServiceCollectionExtensions
{
    public static IServiceCollection AddMapsterObjectMapping(
        this IServiceCollection services,
        Action<TypeAdapterConfig>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddObjectMapping();

        var config = TypeAdapterConfig.GlobalSettings;
        configure?.Invoke(config);

        services.AddSingleton(config);
        services.Replace(ServiceDescriptor.Singleton<IObjectMappingProvider, MapsterObjectMappingProvider>());

        return services;
    }
}
