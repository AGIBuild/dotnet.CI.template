using System;

namespace ChengYuan.BlobStoring;

public class BlobProviderArgs
{
    public BlobProviderArgs(string containerName, string blobName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);

        ContainerName = containerName;
        BlobName = blobName;
    }

    public string ContainerName { get; }

    public string BlobName { get; }
}
