using System.Reflection;

namespace ChengYuan.Core.Modularity;

public sealed class ModuleDescriptor : IModuleDescriptor
{
    internal ModuleDescriptor(
        Type moduleType,
        ModuleBase instance,
        IReadOnlyList<IModuleDescriptor> dependencies,
        bool isRoot)
    {
        ModuleType = moduleType;
        Instance = instance;
        Name = moduleType.Name;
        Assembly = moduleType.Assembly;
        AdditionalAssemblies = [];
        Dependencies = dependencies;
        IsRoot = isRoot;
    }

    public Type ModuleType { get; }

    public string Name { get; }

    public Assembly Assembly { get; }

    public IReadOnlyList<Assembly> AdditionalAssemblies { get; }

    public IReadOnlyList<IModuleDescriptor> Dependencies { get; }

    public bool IsRoot { get; }

    internal ModuleBase Instance { get; }
}
