using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace ChengYuan.DistributedLocking;

public static class RedisDistributedLockServiceCollectionExtensions
{
    public static IServiceCollection AddRedisDistributedLock(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.TryAddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connectionString));
        services.AddSingleton<IDistributedLock, RedisDistributedLock>();

        return services;
    }

    public static IServiceCollection AddRedisDistributedLock(this IServiceCollection services, IConnectionMultiplexer connectionMultiplexer)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(connectionMultiplexer);

        services.TryAddSingleton(connectionMultiplexer);
        services.AddSingleton<IDistributedLock, RedisDistributedLock>();

        return services;
    }
}
