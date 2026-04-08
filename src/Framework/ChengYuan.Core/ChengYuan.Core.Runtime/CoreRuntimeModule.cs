using ChengYuan.Core.Lifecycle;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Core;

public sealed class CoreRuntimeModule : ModuleBase, IPreConfigureServices
{
    public void PreConfigureServices(IServiceCollection services)
    {
        services.AddCoreRuntime();
    }
}
