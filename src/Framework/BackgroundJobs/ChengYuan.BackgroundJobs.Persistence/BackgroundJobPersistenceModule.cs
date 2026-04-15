using ChengYuan.Core.Modularity;

namespace ChengYuan.BackgroundJobs;

[DependsOn(typeof(BackgroundJobsModule))]
public sealed class BackgroundJobPersistenceModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddBackgroundJobPersistence();
    }
}
