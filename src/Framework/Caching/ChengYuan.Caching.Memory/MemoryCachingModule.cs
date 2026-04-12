using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Caching;

[DependsOn(typeof(CachingModule))]
public sealed class MemoryCachingModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMemoryCache();
        context.Services.AddSingleton<IChengYuanCacheStore, MemoryCacheStore>();
    }
}
