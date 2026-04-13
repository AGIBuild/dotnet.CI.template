using ChengYuan.Core.Modularity;

namespace ChengYuan.BackgroundWorkers;

public sealed class BackgroundWorkersModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddBackgroundWorkerManager();
    }
}
