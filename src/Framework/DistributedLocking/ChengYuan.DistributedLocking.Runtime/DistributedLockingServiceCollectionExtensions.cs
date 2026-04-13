using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.DistributedLocking;

public static class DistributedLockingServiceCollectionExtensions
{
    public static IServiceCollection AddLocalDistributedLocking(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IDistributedLock, LocalDistributedLock>();

        return services;
    }
}
