using ChengYuan.Core.Data;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.EntityFrameworkCore;

[DependsOn(typeof(DataModule))]
public sealed class ChengYuanEntityFrameworkCoreModule : FrameworkCoreModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddOptions<ChengYuanDbContextOptions>();
    }
}
