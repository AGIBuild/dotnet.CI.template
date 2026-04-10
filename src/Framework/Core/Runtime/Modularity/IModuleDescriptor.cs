using System.Reflection;

namespace ChengYuan.Core.Modularity;

public interface IModuleDescriptor
{
    Type ModuleType { get; }

    string Name { get; }

    Assembly Assembly { get; }

    IReadOnlyList<Assembly> AdditionalAssemblies { get; }

    IReadOnlyList<IModuleDescriptor> Dependencies { get; }

    bool IsRoot { get; }
}
