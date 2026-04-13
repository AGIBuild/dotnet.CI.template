using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Core.SimpleStateChecking;

internal sealed class SimpleStateCheckerManager(IServiceProvider serviceProvider) : ISimpleStateCheckerManager
{
    public async Task<bool> IsEnabledAsync<TState>(TState state, CancellationToken cancellationToken = default)
        where TState : IHasSimpleStateCheckers
    {
        ArgumentNullException.ThrowIfNull(state);

        var context = new SimpleStateCheckerContext<IHasSimpleStateCheckers>(state, serviceProvider);

        foreach (var checker in state.StateCheckers)
        {
            if (!await checker.IsEnabledAsync(context, cancellationToken))
            {
                return false;
            }
        }

        return true;
    }
}
