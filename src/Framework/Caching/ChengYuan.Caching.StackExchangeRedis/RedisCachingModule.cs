using ChengYuan.Core.Modularity;

namespace ChengYuan.Caching;

[DependsOn(typeof(CachingModule))]
public sealed class RedisCachingModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddRedisCache();
    }
}
