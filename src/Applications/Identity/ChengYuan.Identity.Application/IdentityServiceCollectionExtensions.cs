using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Identity;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<UserManager>();
        services.TryAddScoped<IUserManager>(serviceProvider => serviceProvider.GetRequiredService<UserManager>());
        services.TryAddScoped<IUserReader>(serviceProvider => serviceProvider.GetRequiredService<UserManager>());
        services.TryAddScoped<RoleManager>();
        services.TryAddScoped<IRoleManager>(serviceProvider => serviceProvider.GetRequiredService<RoleManager>());
        services.TryAddScoped<IRoleReader>(serviceProvider => serviceProvider.GetRequiredService<RoleManager>());

        return services;
    }
}
