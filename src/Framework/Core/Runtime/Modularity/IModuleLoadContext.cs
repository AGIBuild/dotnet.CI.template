namespace ChengYuan.Core.Modularity;

public interface IModuleLoadContext
{
    IModuleDescriptor CurrentModule { get; }

    IModuleCatalog ModuleCatalog { get; }

    IReadOnlyList<IModuleDescriptor> Dependents { get; }
}
