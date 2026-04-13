using ChengYuan.Core.Modularity;

namespace ChengYuan.Emailing;

public sealed class EmailingModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddEmailing();
    }
}
