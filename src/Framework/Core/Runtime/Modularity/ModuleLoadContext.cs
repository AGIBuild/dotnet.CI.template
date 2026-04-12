namespace ChengYuan.Core.Modularity;

public sealed class ModuleLoadContext(
    IModuleDescriptor currentModule,
    IModuleCatalog moduleCatalog,
    IReadOnlyList<IModuleDescriptor> dependents) : IModuleLoadContext
{
    public IModuleDescriptor CurrentModule { get; } = currentModule;

    public IModuleCatalog ModuleCatalog { get; } = moduleCatalog;

    public IReadOnlyList<IModuleDescriptor> Dependents { get; } = dependents;
}
