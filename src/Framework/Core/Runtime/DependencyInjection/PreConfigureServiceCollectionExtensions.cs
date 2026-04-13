using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Core.DependencyInjection;

public static class PreConfigureServiceCollectionExtensions
{
    public static IServiceCollection PreConfigure<TOptions>(this IServiceCollection services, Action<TOptions> configureAction)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureAction);

        services.GetPreConfigureActions<TOptions>().Add(configureAction);
        return services;
    }

    public static TOptions ExecutePreConfiguredActions<TOptions>(this IServiceCollection services)
        where TOptions : new()
    {
        return services.ExecutePreConfiguredActions(new TOptions());
    }

    public static TOptions ExecutePreConfiguredActions<TOptions>(this IServiceCollection services, TOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.GetPreConfigureActions<TOptions>().Configure(options);
        return options;
    }

    public static PreConfigureActionList<TOptions> GetPreConfigureActions<TOptions>(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var accessor = services.GetObjectAccessorOrNull<TOptions>();
        if (accessor is not null)
        {
            return accessor;
        }

        var actionList = new PreConfigureActionList<TOptions>();
        services.AddObjectAccessor(actionList);
        return actionList;
    }

    private static PreConfigureActionList<TOptions>? GetObjectAccessorOrNull<TOptions>(this IServiceCollection services)
    {
        return services
            .FirstOrDefault(d => d.ServiceType == typeof(ObjectAccessor<PreConfigureActionList<TOptions>>))
            ?.ImplementationInstance is ObjectAccessor<PreConfigureActionList<TOptions>> accessor
            ? accessor.Value
            : null;
    }

    private static void AddObjectAccessor<TOptions>(this IServiceCollection services, PreConfigureActionList<TOptions> actionList)
    {
        var accessor = new ObjectAccessor<PreConfigureActionList<TOptions>>(actionList);
        // CA2263: Generic overload is unavailable for open type parameters.
#pragma warning disable CA2263
        services.Insert(0, ServiceDescriptor.Singleton(typeof(ObjectAccessor<PreConfigureActionList<TOptions>>), accessor));
#pragma warning restore CA2263
    }
}
