using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Core.SimpleStateChecking;

public interface ISimpleStateCheckerManager
{
    Task<bool> IsEnabledAsync<TState>(TState state, CancellationToken cancellationToken = default)
        where TState : IHasSimpleStateCheckers;
}
