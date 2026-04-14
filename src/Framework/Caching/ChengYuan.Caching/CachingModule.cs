using System;
using ChengYuan.Core.Modularity;
using ChengYuan.MultiTenancy;

namespace ChengYuan.Caching;

[DependsOn(typeof(MultiTenancyModule))]
public sealed class CachingModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddCaching();

        Configure<ChengYuanCacheOptions>(options =>
        {
            options.GlobalCacheEntryOptions = new ChengYuanCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(20)
            };
        });
    }
}
