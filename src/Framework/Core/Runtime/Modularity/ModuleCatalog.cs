using System.Diagnostics.CodeAnalysis;

namespace ChengYuan.Core.Modularity;

public sealed class ModuleCatalog : IModuleCatalog
{
    private readonly ModuleDescriptor[] _modules;
    private readonly Dictionary<Type, ModuleDescriptor> _moduleMap;

    internal ModuleCatalog(IReadOnlyList<ModuleDescriptor> modules)
    {
        _modules = modules.ToArray();
        ModuleTypes = _modules.Select(static module => module.ModuleType).ToArray();
        _moduleMap = _modules.ToDictionary(static module => module.ModuleType);
    }

    public IReadOnlyList<IModuleDescriptor> Modules => _modules;

    public IReadOnlyList<Type> ModuleTypes { get; }

    public IModuleDescriptor GetModule(Type moduleType)
    {
        ArgumentNullException.ThrowIfNull(moduleType);

        if (!_moduleMap.TryGetValue(moduleType, out var descriptor))
        {
            throw new InvalidOperationException($"Module '{moduleType.FullName}' is not loaded.");
        }

        return descriptor;
    }

    public bool TryGetModule(Type moduleType, [NotNullWhen(true)] out IModuleDescriptor? descriptor)
    {
        ArgumentNullException.ThrowIfNull(moduleType);

        if (_moduleMap.TryGetValue(moduleType, out var concrete))
        {
            descriptor = concrete;
            return true;
        }

        descriptor = null;
        return false;
    }

    public bool IsLoaded<TModule>() where TModule : ModuleBase
    {
        return _moduleMap.ContainsKey(typeof(TModule));
    }

    public IReadOnlyList<IModuleDescriptor> GetInitializationOrder() => _modules;

    public IReadOnlyList<IModuleDescriptor> GetShutdownOrder() => _modules.Reverse().ToArray();

    internal IReadOnlyList<ModuleDescriptor> ConcreteModules => _modules;
}
