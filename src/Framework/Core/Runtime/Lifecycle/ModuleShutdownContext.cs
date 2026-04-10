using ChengYuan.Core.Modularity;

namespace ChengYuan.Core.Lifecycle;

public sealed class ModuleShutdownContext(
    IServiceProvider serviceProvider,
    IModuleCatalog moduleCatalog,
    CancellationToken cancellationToken) : IModuleShutdownContext
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public IModuleCatalog ModuleCatalog { get; } = moduleCatalog;

    public CancellationToken CancellationToken { get; } = cancellationToken;
}
