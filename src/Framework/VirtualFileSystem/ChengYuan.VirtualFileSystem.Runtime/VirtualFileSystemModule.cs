using ChengYuan.Core.Modularity;

namespace ChengYuan.VirtualFileSystem;

public sealed class VirtualFileSystemModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddVirtualFileSystem();
    }
}
