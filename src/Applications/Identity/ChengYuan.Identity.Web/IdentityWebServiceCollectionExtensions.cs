using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Identity;

public static class IdentityWebServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityWeb(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }
}
