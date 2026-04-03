namespace ChengYuan.Core.Modularity;

public interface IModuleDescriptor
{
    Type ModuleType { get; }

    IReadOnlyList<Type> DependencyTypes { get; }
}

public sealed record ModuleDescriptor(Type ModuleType, IReadOnlyList<Type> DependencyTypes) : IModuleDescriptor;

public sealed class ModuleCatalog
{
    public ModuleCatalog(IReadOnlyList<ModuleDescriptor> modules)
    {
        Modules = modules;
        ModuleTypes = modules.Select(static module => module.ModuleType).ToArray();
    }

    public IReadOnlyList<ModuleDescriptor> Modules { get; }

    public IReadOnlyList<Type> ModuleTypes { get; }
}
