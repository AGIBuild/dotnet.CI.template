using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Authorization;

public interface IPermissionGrantProvider
{
    string Name { get; }

    int Order { get; }

    ValueTask<PermissionGrant?> GetOrNullAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default);
}
