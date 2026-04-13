using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Core.SimpleStateChecking;

public interface ISimpleStateChecker<TState>
    where TState : IHasSimpleStateCheckers
{
    Task<bool> IsEnabledAsync(SimpleStateCheckerContext<TState> context, CancellationToken cancellationToken = default);
}
