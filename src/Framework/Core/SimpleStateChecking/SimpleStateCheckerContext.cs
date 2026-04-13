using System;

namespace ChengYuan.Core.SimpleStateChecking;

public sealed class SimpleStateCheckerContext<TState>(TState state, IServiceProvider serviceProvider)
    where TState : IHasSimpleStateCheckers
{
    public TState State { get; } = state ?? throw new ArgumentNullException(nameof(state));

    public IServiceProvider ServiceProvider { get; } = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
}
