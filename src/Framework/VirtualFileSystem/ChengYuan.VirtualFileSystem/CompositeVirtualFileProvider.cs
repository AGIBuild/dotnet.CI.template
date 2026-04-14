using System;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Composite;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace ChengYuan.VirtualFileSystem;

internal sealed class CompositeVirtualFileProvider : IVirtualFileProvider
{
    private readonly Lazy<IFileProvider> _inner;

    public CompositeVirtualFileProvider(IOptions<VirtualFileSystemOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _inner = new Lazy<IFileProvider>(() => BuildProvider(options.Value));
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        return _inner.Value.GetDirectoryContents(subpath);
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        return _inner.Value.GetFileInfo(subpath);
    }

    public IChangeToken Watch(string filter)
    {
        return _inner.Value.Watch(filter);
    }

    private static IFileProvider BuildProvider(VirtualFileSystemOptions options)
    {
        var providers = options.FileSets
            .Select(fs => new EmbeddedFileProvider(fs.Assembly, fs.RootNamespace))
            .Cast<IFileProvider>()
            .ToList();

        return providers.Count switch
        {
            0 => new NullFileProvider(),
            1 => providers[0],
            _ => new CompositeFileProvider(providers),
        };
    }
}
