using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.ObjectMapping;

public static class ObjectMappingServiceCollectionExtensions
{
    public static IServiceCollection AddObjectMapping(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IObjectMappingProvider, NotConfiguredObjectMappingProvider>();
        services.TryAddSingleton<IObjectMapper, DefaultObjectMapper>();
        return services;
    }
}
