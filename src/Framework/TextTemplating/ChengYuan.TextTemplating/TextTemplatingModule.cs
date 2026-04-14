using ChengYuan.Core.Modularity;

namespace ChengYuan.TextTemplating;

public sealed class TextTemplatingModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTextTemplating();
    }
}
