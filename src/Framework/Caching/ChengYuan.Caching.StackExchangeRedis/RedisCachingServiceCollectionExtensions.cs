using System;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Caching;

public static class RedisCachingServiceCollectionExtensions
{
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        Action<RedisCacheOptions>? configure = null)
    {
        var redisCacheOptions = new RedisCacheOptions();
        configure?.Invoke(redisCacheOptions);

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisCacheOptions.Configuration;
            options.InstanceName = redisCacheOptions.InstanceName;
        });

        services.AddSingleton<IChengYuanCacheStore, DistributedCacheStore>();

        return services;
    }
}
