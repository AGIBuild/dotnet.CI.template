using System.Diagnostics.CodeAnalysis;

namespace ChengYuan.Core.Modularity;

public interface IModuleCatalog
{
    IReadOnlyList<IModuleDescriptor> Modules { get; }

    IReadOnlyList<Type> ModuleTypes { get; }

    IModuleDescriptor GetModule(Type moduleType);

    bool TryGetModule(Type moduleType, [NotNullWhen(true)] out IModuleDescriptor? descriptor);

    bool IsLoaded<TModule>() where TModule : ModuleBase;

    IReadOnlyList<IModuleDescriptor> GetInitializationOrder();

    IReadOnlyList<IModuleDescriptor> GetShutdownOrder();
}
