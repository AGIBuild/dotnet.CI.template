using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.BlobStoring;

internal sealed class BlobContainer<TContainer>(IBlobProvider provider) : IBlobContainer<TContainer>
    where TContainer : class
{
    private readonly string _containerName = BlobContainerNameAttribute.GetContainerName<TContainer>();

    public Task SaveAsync(string name, Stream stream, bool overrideExisting = false, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(stream);

        return provider.SaveAsync(
            new BlobProviderSaveArgs(_containerName, name, stream, overrideExisting),
            cancellationToken);
    }

    public Task<Stream?> GetOrNullAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return provider.GetOrNullAsync(
            new BlobProviderArgs(_containerName, name),
            cancellationToken);
    }

    public Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return provider.ExistsAsync(
            new BlobProviderArgs(_containerName, name),
            cancellationToken);
    }

    public Task DeleteAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return provider.DeleteAsync(
            new BlobProviderArgs(_containerName, name),
            cancellationToken);
    }
}
