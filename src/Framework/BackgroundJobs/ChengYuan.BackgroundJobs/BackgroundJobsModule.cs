using ChengYuan.BackgroundWorkers;
using ChengYuan.Core.Modularity;

namespace ChengYuan.BackgroundJobs;

[DependsOn(typeof(BackgroundWorkersModule))]
public sealed class BackgroundJobsModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddBackgroundJobs();
    }
}
