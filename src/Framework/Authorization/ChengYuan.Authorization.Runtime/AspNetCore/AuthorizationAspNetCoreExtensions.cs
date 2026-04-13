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
        services.TryAddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IAuthorizationHandler, PermissionAuthorizationHandler>());

        return services;
    }
}
