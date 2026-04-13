using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.BlobStoring;

public interface IBlobProvider
{
    Task SaveAsync(BlobProviderSaveArgs args, CancellationToken cancellationToken = default);

    Task<Stream?> GetOrNullAsync(BlobProviderArgs args, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(BlobProviderArgs args, CancellationToken cancellationToken = default);

    Task DeleteAsync(BlobProviderArgs args, CancellationToken cancellationToken = default);
}
