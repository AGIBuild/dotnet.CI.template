using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Core.DependencyInjection;

public static class ConventionalRegistrationExtensions
{
    public static IServiceCollection AddConventionalServices(this IServiceCollection services, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);

        var types = assembly.GetTypes()
            .Where(static type => type is { IsClass: true, IsAbstract: false, IsGenericType: false })
            .Where(static type => !type.IsDefined(typeof(DisableConventionalRegistrationAttribute), inherit: true));

        foreach (var type in types)
        {
            var lifetime = ResolveLifetime(type);
            if (lifetime is null)
            {
                continue;
            }

            var serviceTypes = GetExposedServiceTypes(type);

            foreach (var serviceType in serviceTypes)
            {
                services.TryAdd(ServiceDescriptor.Describe(serviceType, type, lifetime.Value));
            }
        }

        return services;
    }

    private static ServiceLifetime? ResolveLifetime(Type type)
    {
        if (typeof(ITransientService).IsAssignableFrom(type))
        {
            return ServiceLifetime.Transient;
        }

        if (typeof(IScopedService).IsAssignableFrom(type))
        {
            return ServiceLifetime.Scoped;
        }

        if (typeof(ISingletonService).IsAssignableFrom(type))
        {
            return ServiceLifetime.Singleton;
        }

        return null;
    }

    private static List<Type> GetExposedServiceTypes(Type implementationType)
    {
        var attribute = implementationType.GetCustomAttribute<ExposeServicesAttribute>();
        if (attribute is not null && attribute.ServiceTypes.Length > 0)
        {
            return [.. attribute.ServiceTypes];
        }

        List<Type> serviceTypes = [];

        foreach (var interfaceType in implementationType.GetInterfaces())
        {
            if (interfaceType == typeof(ITransientService) ||
                interfaceType == typeof(IScopedService) ||
                interfaceType == typeof(ISingletonService))
            {
                continue;
            }

            var interfaceName = interfaceType.IsGenericType
                ? interfaceType.Name[..interfaceType.Name.IndexOf('`')]
                : interfaceType.Name;

            if (interfaceName.StartsWith('I'))
            {
                interfaceName = interfaceName[1..];
            }

            if (implementationType.Name.EndsWith(interfaceName, StringComparison.OrdinalIgnoreCase))
            {
                serviceTypes.Add(interfaceType);
            }
        }

        if (serviceTypes.Count == 0)
        {
            serviceTypes.Add(implementationType);
        }

        return serviceTypes;
    }
}
