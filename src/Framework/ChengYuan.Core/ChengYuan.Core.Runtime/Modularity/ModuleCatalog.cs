using System;
using System.Collections.Generic;
using System.Linq;

namespace ChengYuan.Core.Modularity;

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
