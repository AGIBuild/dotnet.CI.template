using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.BlobStoring;

public interface IBlobContainer
{
    Task SaveAsync(string name, Stream stream, bool overrideExisting = false, CancellationToken cancellationToken = default);

    Task<Stream?> GetOrNullAsync(string name, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);

    Task DeleteAsync(string name, CancellationToken cancellationToken = default);
}
