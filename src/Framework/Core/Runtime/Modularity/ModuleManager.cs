using ChengYuan.Core.Lifecycle;

namespace ChengYuan.Core.Modularity;

public sealed class ModuleManager : IModuleManager
{
    private readonly ModuleCatalog _catalog;
    private readonly IServiceProvider _serviceProvider;
    private int _initializeState; // 0 = not started, 1 = initialized

    internal ModuleManager(ModuleCatalog catalog, IServiceProvider serviceProvider)
    {
        _catalog = catalog;
        _serviceProvider = serviceProvider;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref _initializeState, 1, 0) != 0)
        {
            throw new InvalidOperationException("Module initialization has already been executed.");
        }

        var context = new ModuleInitializationContext(_serviceProvider, _catalog, cancellationToken);

        foreach (var descriptor in _catalog.ConcreteModules)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (descriptor.Instance is IOnModulePreInitialize preInitialize)
                {
                    await preInitialize.PreInitializeAsync(context);
                }

                if (descriptor.Instance is IOnModuleInitialize initialize)
                {
                    await initialize.InitializeAsync(context);
                }

                if (descriptor.Instance is IOnModulePostInitialize postInitialize)
                {
                    await postInitialize.PostInitializeAsync(context);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"An error occurred during initialization of module '{descriptor.Name}'.", ex);
            }
        }
    }

    public async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        if (_initializeState == 0)
        {
            return;
        }

        var context = new ModuleShutdownContext(_serviceProvider, _catalog, cancellationToken);

        foreach (var descriptor in _catalog.ConcreteModules.Reverse())
        {
            try
            {
                if (descriptor.Instance is IOnModuleShutdown shutdown)
                {
                    await shutdown.ShutdownAsync(context);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"An error occurred during shutdown of module '{descriptor.Name}'.", ex);
            }
        }
    }
}
