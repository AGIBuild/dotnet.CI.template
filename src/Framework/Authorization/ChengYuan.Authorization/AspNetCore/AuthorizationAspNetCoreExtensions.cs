using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Authorization;

public static class AuthorizationAspNetCoreExtensions
{
    public static IServiceCollection AddChengYuanAuthorization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAuthorizationCore();

        // Replace the default policy provider with our permission-aware one.
        // Must use Replace because AddAuthorization() may have already registered DefaultAuthorizationPolicyProvider.
        services.Replace(ServiceDescriptor.Singleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IAuthorizationHandler, PermissionAuthorizationHandler>());

        return services;
    }
}
