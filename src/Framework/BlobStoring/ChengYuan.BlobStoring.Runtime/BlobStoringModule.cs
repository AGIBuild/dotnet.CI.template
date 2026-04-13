using ChengYuan.Core.Modularity;

namespace ChengYuan.BlobStoring;

public sealed class BlobStoringModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddBlobStoring();
    }
}
