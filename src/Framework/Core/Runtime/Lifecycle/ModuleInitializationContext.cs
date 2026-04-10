using ChengYuan.Core.Modularity;

namespace ChengYuan.Core.Lifecycle;

public sealed class ModuleInitializationContext(
    IServiceProvider serviceProvider,
    IModuleCatalog moduleCatalog,
    CancellationToken cancellationToken) : IModuleInitializationContext
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public IModuleCatalog ModuleCatalog { get; } = moduleCatalog;

    public CancellationToken CancellationToken { get; } = cancellationToken;
}
