using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Caching;

public static class CachingServiceCollectionExtensions
{
    public static IServiceCollection AddCaching(this IServiceCollection services)
    {
        services.AddSingleton<IChengYuanCacheKeyNormalizer, DefaultChengYuanCacheKeyNormalizer>();
        services.AddSingleton<IChengYuanCacheSerializer, SystemTextJsonChengYuanCacheSerializer>();
        services.AddSingleton<IChengYuanCache, DefaultChengYuanCache>();
        services.AddSingleton(typeof(IChengYuanCache<>), typeof(DefaultChengYuanTypedCache<>));

        return services;
    }
}
