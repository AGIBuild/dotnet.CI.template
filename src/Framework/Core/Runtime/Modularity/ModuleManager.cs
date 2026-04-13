using ChengYuan.Core.Lifecycle;
using ChengYuan.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

        ReplayInitLogs();

        var contributors = ResolveLifecycleContributors();
        var context = new ModuleInitializationContext(_serviceProvider, _catalog, cancellationToken);

        foreach (var descriptor in _catalog.ConcreteModules)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await descriptor.Instance.PreInitializeAsync(context);
                await descriptor.Instance.InitializeAsync(context);
                await descriptor.Instance.PostInitializeAsync(context);

                foreach (var contributor in contributors)
                {
                    await contributor.InitializeAsync(context, descriptor, descriptor.Instance);
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

        var contributors = ResolveLifecycleContributors();
        var context = new ModuleShutdownContext(_serviceProvider, _catalog, cancellationToken);

        foreach (var descriptor in _catalog.ConcreteModules.Reverse())
        {
            try
            {
                foreach (var contributor in contributors)
                {
                    await contributor.ShutdownAsync(context, descriptor, descriptor.Instance);
                }

                await descriptor.Instance.ShutdownAsync(context);
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

    private IModuleLifecycleContributor[] ResolveLifecycleContributors()
    {
        var options = _serviceProvider.GetService<IOptions<ModuleLifecycleOptions>>()?.Value;
        if (options is null || options.Contributors.Count == 0)
        {
            return [];
        }

        return options.Contributors
            .Select(type => (IModuleLifecycleContributor)ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, type))
            .ToArray();
    }

    private void ReplayInitLogs()
    {
        var initLoggerFactory = _serviceProvider.GetService<IInitLoggerFactory>();
        if (initLoggerFactory is null)
        {
            return;
        }

        var entries = initLoggerFactory.GetAllEntries();
        if (entries.Count == 0)
        {
            return;
        }

        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();

        foreach (var entry in entries)
        {
            var logger = loggerFactory.CreateLogger(entry.CategoryName);
#pragma warning disable CA1848, CA2254 // Init log entries are pre-formatted during module loading
            logger.Log(entry.LogLevel, entry.EventId, entry.Exception, entry.Message);
#pragma warning restore CA1848, CA2254
        }

        initLoggerFactory.ClearAllEntries();
    }
}
