using ChengYuan.Core.Modularity;

namespace ChengYuan.Core.Lifecycle;

public interface IModuleInitializationContext
{
    IServiceProvider ServiceProvider { get; }

    IModuleCatalog ModuleCatalog { get; }

    CancellationToken CancellationToken { get; }
}
