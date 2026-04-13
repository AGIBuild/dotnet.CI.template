using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Core.Data;

public interface IConnectionStringResolver
{
    Task<string?> ResolveAsync(string? connectionStringName = null, CancellationToken cancellationToken = default);
}
