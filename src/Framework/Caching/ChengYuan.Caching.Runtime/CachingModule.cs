using ChengYuan.Core.Modularity;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Caching;

[DependsOn(typeof(MultiTenancyModule))]
public sealed class CachingModule : ModuleBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddCaching();
    }
}
