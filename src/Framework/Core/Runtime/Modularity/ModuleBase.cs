using ChengYuan.Core.Lifecycle;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Core.Modularity;

public abstract class ModuleBase
{
    private IModuleDescriptor? _currentModule;
    private IModuleCatalog? _moduleCatalog;
    private IReadOnlyList<IModuleDescriptor> _dependencies = [];
    private IReadOnlyList<IModuleDescriptor> _dependents = [];
    private ServiceConfigurationContext? _serviceConfigurationContext;

    public virtual void OnLoaded(IModuleLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        CacheTopology(context);
        ValidateLoad(context);
        OnModuleLoaded(context);
    }

    protected IModuleDescriptor CurrentModule => _currentModule ?? throw CreateTopologyAccessException(nameof(CurrentModule));

    protected IModuleCatalog ModuleCatalog => _moduleCatalog ?? throw CreateTopologyAccessException(nameof(ModuleCatalog));

    protected IReadOnlyList<IModuleDescriptor> Dependencies => _dependencies;

    protected IReadOnlyList<IModuleDescriptor> Dependents => _dependents;

    protected ModuleCategory Category => CurrentModule.Category;

    protected bool IsRoot => CurrentModule.IsRoot;

    protected internal ServiceConfigurationContext ServiceConfigurationContext
    {
        get => _serviceConfigurationContext ?? throw new InvalidOperationException(
            $"{nameof(ServiceConfigurationContext)} is only available in the " +
            $"{nameof(PreConfigureServices)}, {nameof(ConfigureServices)}, and {nameof(PostConfigureServices)} methods.");
        internal set => _serviceConfigurationContext = value;
    }

    protected virtual void ValidateLoad(IModuleLoadContext context)
    {
    }

    protected virtual void OnModuleLoaded(IModuleLoadContext context)
    {
    }

    public virtual void PreConfigureServices(ServiceConfigurationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
    }

    public virtual void ConfigureServices(ServiceConfigurationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
    }

    public virtual void PostConfigureServices(ServiceConfigurationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
    }

    public virtual async Task PreInitializeAsync(IModuleInitializationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        await OnPreInitializeAsync(context);
    }

    protected virtual Task OnPreInitializeAsync(IModuleInitializationContext context)
    {
        return Task.CompletedTask;
    }

    public virtual async Task InitializeAsync(IModuleInitializationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        await OnInitializeAsync(context);
    }

    protected virtual Task OnInitializeAsync(IModuleInitializationContext context)
    {
        return Task.CompletedTask;
    }

    public virtual async Task PostInitializeAsync(IModuleInitializationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        await OnPostInitializeAsync(context);
    }

    protected virtual Task OnPostInitializeAsync(IModuleInitializationContext context)
    {
        return Task.CompletedTask;
    }

    public virtual async Task ShutdownAsync(IModuleShutdownContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        await OnShutdownAsync(context);
    }

    protected virtual Task OnShutdownAsync(IModuleShutdownContext context)
    {
        return Task.CompletedTask;
    }

    protected bool HasDependencyCategory(params ModuleCategory[] categories)
    {
        ArgumentNullException.ThrowIfNull(categories);

        return Dependencies.Any(dependency => categories.Contains(dependency.Category));
    }

    protected bool HasDependentCategory(params ModuleCategory[] categories)
    {
        ArgumentNullException.ThrowIfNull(categories);

        return Dependents.Any(dependent => categories.Contains(dependent.Category));
    }

    protected IReadOnlyList<IModuleDescriptor> GetDependencies(params ModuleCategory[] categories)
    {
        ArgumentNullException.ThrowIfNull(categories);

        return Dependencies
            .Where(dependency => categories.Contains(dependency.Category))
            .ToArray();
    }

    protected IReadOnlyList<IModuleDescriptor> GetDependents(params ModuleCategory[] categories)
    {
        ArgumentNullException.ThrowIfNull(categories);

        return Dependents
            .Where(dependent => categories.Contains(dependent.Category))
            .ToArray();
    }

    protected void Configure<TOptions>(Action<TOptions> configureOptions)
        where TOptions : class
    {
        ServiceConfigurationContext.Services.Configure(configureOptions);
    }

    protected void PostConfigure<TOptions>(Action<TOptions> configureOptions)
        where TOptions : class
    {
        ServiceConfigurationContext.Services.PostConfigure(configureOptions);
    }

    protected void PostConfigureAll<TOptions>(Action<TOptions> configureOptions)
        where TOptions : class
    {
        ServiceConfigurationContext.Services.PostConfigureAll(configureOptions);
    }

    private void CacheTopology(IModuleLoadContext context)
    {
        _currentModule = context.CurrentModule;
        _moduleCatalog = context.ModuleCatalog;
        _dependencies = context.CurrentModule.Dependencies;
        _dependents = context.Dependents;
    }

    private static InvalidOperationException CreateTopologyAccessException(string memberName)
    {
        return new InvalidOperationException($"Module topology is not available before load has completed. Cannot access '{memberName}'.");
    }
}
