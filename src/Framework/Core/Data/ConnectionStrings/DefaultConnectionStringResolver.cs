using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace ChengYuan.Core.Data;

public sealed class DefaultConnectionStringResolver(IOptions<ConnectionStringOptions> options) : IConnectionStringResolver
{
    public Task<string?> ResolveAsync(string? connectionStringName = null, CancellationToken cancellationToken = default)
    {
        if (connectionStringName is null)
        {
            return Task.FromResult(options.Value.Default);
        }

        return options.Value.ConnectionStrings.TryGetValue(connectionStringName, out var connectionString)
            ? Task.FromResult<string?>(connectionString)
            : Task.FromResult(options.Value.Default);
    }
}
