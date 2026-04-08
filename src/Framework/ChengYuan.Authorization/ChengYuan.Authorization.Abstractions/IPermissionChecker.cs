using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Authorization;

public interface IPermissionChecker
{
    ValueTask<bool> IsGrantedAsync(string name, CancellationToken cancellationToken = default);
}
