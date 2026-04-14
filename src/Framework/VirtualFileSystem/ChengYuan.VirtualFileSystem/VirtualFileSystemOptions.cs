using System.Collections.Generic;
using System.Reflection;

namespace ChengYuan.VirtualFileSystem;

public sealed class VirtualFileSystemOptions
{
    public IList<VirtualFileSetInfo> FileSets { get; } = [];

    public void AddEmbedded<T>()
    {
        AddEmbedded(typeof(T).Assembly);
    }

    public void AddEmbedded(Assembly assembly, string? rootNamespace = null)
    {
        FileSets.Add(new VirtualFileSetInfo(assembly, rootNamespace));
    }
}
