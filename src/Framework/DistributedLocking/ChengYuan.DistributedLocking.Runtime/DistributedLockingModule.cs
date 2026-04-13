using ChengYuan.Core.Modularity;

namespace ChengYuan.DistributedLocking;

public sealed class DistributedLockingModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddLocalDistributedLocking();
    }
}
