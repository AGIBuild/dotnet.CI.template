using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Authorization;

public static class AuthorizationServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorizationCore(this IServiceCollection services)
    {
        services.TryAddSingleton<IPermissionDefinitionManager, DefaultPermissionDefinitionManager>();
        services.TryAddSingleton<IPermissionChecker, DefaultPermissionChecker>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPermissionGrantProvider, UserPermissionGrantProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPermissionGrantProvider, TenantPermissionGrantProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPermissionGrantProvider, GlobalPermissionGrantProvider>());

        return services;
    }

    public static IServiceCollection AddInMemoryPermissions(this IServiceCollection services, Action<InMemoryPermissionsBuilder>? configure = null)
    {
        var builder = new InMemoryPermissionsBuilder();
        configure?.Invoke(builder);

        services.AddSingleton<IPermissionGrantProvider>(new InMemoryGlobalPermissionGrantProvider(builder.GlobalValues));
        services.AddSingleton<IPermissionGrantProvider>(new InMemoryTenantPermissionGrantProvider(builder.TenantValues));
        services.AddSingleton<IPermissionGrantProvider>(new InMemoryUserPermissionGrantProvider(builder.UserValues));

        return services;
    }
}
