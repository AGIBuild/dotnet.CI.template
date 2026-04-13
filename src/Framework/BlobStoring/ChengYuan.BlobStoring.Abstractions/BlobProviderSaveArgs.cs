using System.IO;

namespace ChengYuan.BlobStoring;

public sealed class BlobProviderSaveArgs(string containerName, string blobName, Stream stream, bool overrideExisting)
    : BlobProviderArgs(containerName, blobName)
{
    public Stream Stream { get; } = stream;

    public bool OverrideExisting { get; } = overrideExisting;
}
