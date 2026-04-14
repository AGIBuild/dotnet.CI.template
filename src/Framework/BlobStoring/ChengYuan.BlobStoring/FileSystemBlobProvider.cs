using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace ChengYuan.BlobStoring;

internal sealed class FileSystemBlobProvider(IOptions<FileSystemBlobProviderOptions> options) : IBlobProvider
{
    private readonly string _basePath = options.Value.BasePath;

    public async Task SaveAsync(BlobProviderSaveArgs args, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(args.ContainerName, args.BlobName);

        if (!args.OverrideExisting && File.Exists(filePath))
        {
            throw new IOException($"Blob '{args.BlobName}' already exists in container '{args.ContainerName}'.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await args.Stream.CopyToAsync(fileStream, cancellationToken);
    }

    public Task<Stream?> GetOrNullAsync(BlobProviderArgs args, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(args.ContainerName, args.BlobName);

        if (!File.Exists(filePath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public Task<bool> ExistsAsync(BlobProviderArgs args, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(args.ContainerName, args.BlobName);
        return Task.FromResult(File.Exists(filePath));
    }

    public Task DeleteAsync(BlobProviderArgs args, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(args.ContainerName, args.BlobName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    private string GetFilePath(string containerName, string blobName)
    {
        var filePath = Path.GetFullPath(Path.Combine(_basePath, containerName, blobName));
        var baseFull = Path.GetFullPath(_basePath);

        if (!filePath.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Blob path '{blobName}' resolves outside the configured base directory.");
        }

        return filePath;
    }
}
