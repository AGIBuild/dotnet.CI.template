namespace ChengYuan.Core.Modularity;

public sealed class ModularApplication : IModularApplication
{
    private readonly IModuleManager _moduleManager;
    private int _initializeState; // 0 = not started, 1 = initialized

    internal ModularApplication(
        IServiceProvider services,
        IModuleCatalog moduleCatalog,
        IModuleManager moduleManager)
    {
        Services = services;
        ModuleCatalog = moduleCatalog;
        _moduleManager = moduleManager;
    }

    public IServiceProvider Services { get; }

    public IModuleCatalog ModuleCatalog { get; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref _initializeState, 1, 0) != 0)
        {
            throw new InvalidOperationException("The modular application has already been initialized.");
        }

        await _moduleManager.InitializeAsync(cancellationToken);
    }

    public async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        if (_initializeState == 0)
        {
            return;
        }

        await _moduleManager.ShutdownAsync(cancellationToken);
    }
}
