using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.BlobStoring;

public static class BlobStoringServiceCollectionExtensions
{
    public static IServiceCollection AddBlobStoring(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<FileSystemBlobProviderOptions>();
        services.TryAddSingleton<IBlobProvider, FileSystemBlobProvider>();
        services.TryAddTransient(typeof(IBlobContainer<>), typeof(BlobContainer<>));
        return services;
    }
}
