using ChengYuan.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Identity;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddConventionalServices(typeof(IdentityServiceCollectionExtensions).Assembly);

        return services;
    }
}
