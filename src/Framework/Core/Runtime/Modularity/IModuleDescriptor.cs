using System;
using System.Collections.Generic;

namespace ChengYuan.Core.Modularity;

public interface IModuleDescriptor
{
    Type ModuleType { get; }

    IReadOnlyList<Type> DependencyTypes { get; }
}
