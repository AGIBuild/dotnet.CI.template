namespace ChengYuan.BlobStoring;

public sealed class FileSystemBlobProviderOptions
{
    public string BasePath { get; set; } = Path.Combine(Path.GetTempPath(), "chengyuan-blobs");
}
