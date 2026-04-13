using System;
using System.Reflection;

namespace ChengYuan.VirtualFileSystem;

public sealed class VirtualFileSetInfo
{
    public VirtualFileSetInfo(Assembly assembly, string? rootNamespace = null)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        Assembly = assembly;
        RootNamespace = rootNamespace ?? assembly.GetName().Name ?? string.Empty;
    }

    public Assembly Assembly { get; }

    public string RootNamespace { get; }
}
