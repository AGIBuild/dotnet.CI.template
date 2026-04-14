using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.VirtualFileSystem;

public static class VirtualFileSystemServiceCollectionExtensions
{
    public static IServiceCollection AddVirtualFileSystem(this IServiceCollection services)
    {
        services.AddOptions<VirtualFileSystemOptions>();
        services.TryAddSingleton<IVirtualFileProvider, CompositeVirtualFileProvider>();
        return services;
    }
}
