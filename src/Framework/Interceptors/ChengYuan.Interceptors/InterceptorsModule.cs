using ChengYuan.Core.Modularity;

namespace ChengYuan.Interceptors;

public sealed class InterceptorsModule : FrameworkCoreModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddInterceptors();
    }
}
