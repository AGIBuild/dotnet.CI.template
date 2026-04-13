using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Core.DependencyInjection;

public static class ObjectAccessorServiceCollectionExtensions
{
    public static ObjectAccessor<T> AddObjectAccessor<T>(this IServiceCollection services)
    {
        return services.AddObjectAccessor(new ObjectAccessor<T>());
    }

    public static ObjectAccessor<T> AddObjectAccessor<T>(this IServiceCollection services, T obj)
    {
        return services.AddObjectAccessor(new ObjectAccessor<T>(obj));
    }

    public static ObjectAccessor<T> AddObjectAccessor<T>(this IServiceCollection services, ObjectAccessor<T> accessor)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(accessor);

        if (services.Any(d => d.ServiceType == typeof(ObjectAccessor<T>)))
        {
            throw new InvalidOperationException(
                $"An ObjectAccessor<{typeof(T).Name}> is already registered. Use TryAddObjectAccessor to avoid duplicates.");
        }

        // CA2263: Generic overload is unavailable for open type parameters.
#pragma warning disable CA2263
        services.Insert(0, ServiceDescriptor.Singleton(typeof(ObjectAccessor<T>), accessor));
        services.Insert(0, ServiceDescriptor.Singleton(typeof(IObjectAccessor<T>), accessor));
#pragma warning restore CA2263

        return accessor;
    }

    public static ObjectAccessor<T> TryAddObjectAccessor<T>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var existing = services.GetObjectAccessorOrNull<T>();
        if (existing is not null)
        {
            return existing;
        }

        return services.AddObjectAccessor<T>();
    }

    public static T? GetObjectOrNull<T>(this IServiceCollection services)
        where T : class
    {
        return services.GetObjectAccessorOrNull<T>()?.Value;
    }

    public static T GetObject<T>(this IServiceCollection services)
        where T : class
    {
        return services.GetObjectOrNull<T>()
            ?? throw new InvalidOperationException(
                $"Could not find an ObjectAccessor<{typeof(T).Name}> in services. " +
                $"Ensure AddObjectAccessor<{typeof(T).Name}> was called before.");
    }

    private static ObjectAccessor<T>? GetObjectAccessorOrNull<T>(this IServiceCollection services)
    {
        return services
            .FirstOrDefault(d => d.ServiceType == typeof(ObjectAccessor<T>))
            ?.ImplementationInstance as ObjectAccessor<T>;
    }
}
