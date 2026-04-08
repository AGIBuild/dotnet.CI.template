using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Caching;

[DependsOn(typeof(CachingModule))]
public sealed class MemoryCachingModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<IChengYuanCacheStore, MemoryCacheStore>();
    }
}
