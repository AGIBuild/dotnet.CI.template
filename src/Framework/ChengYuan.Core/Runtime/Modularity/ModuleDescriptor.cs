using System;
using System.Collections.Generic;

namespace ChengYuan.Core.Modularity;

public sealed record ModuleDescriptor(Type ModuleType, IReadOnlyList<Type> DependencyTypes) : IModuleDescriptor;
