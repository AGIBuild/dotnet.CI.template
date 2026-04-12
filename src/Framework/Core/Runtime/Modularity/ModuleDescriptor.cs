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
        Category = ResolveCategory(instance);
        Name = moduleType.Name;
        Assembly = moduleType.Assembly;
        AdditionalAssemblies = [];
        Dependencies = dependencies;
        IsRoot = isRoot;
    }

    public Type ModuleType { get; }

    public ModuleCategory Category { get; }

    public string Name { get; }

    public Assembly Assembly { get; }

    public IReadOnlyList<Assembly> AdditionalAssemblies { get; }

    public IReadOnlyList<IModuleDescriptor> Dependencies { get; }

    public bool IsRoot { get; }

    internal ModuleBase Instance { get; }

    private static ModuleCategory ResolveCategory(ModuleBase instance) => instance switch
    {
        FrameworkCoreModule => ModuleCategory.FrameworkCore,
        ApplicationModule => ModuleCategory.Application,
        ExtensionModule => ModuleCategory.Extension,
        HostModule => ModuleCategory.Host,
        // Keep direct ModuleBase subclasses working for low-level infrastructure and engine tests.
        _ => ModuleCategory.FrameworkCore
    };
}
