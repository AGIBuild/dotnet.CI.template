using ChengYuan.Core;
using ChengYuan.Core.SimpleStateChecking;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class SimpleStateCheckerManagerTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public SimpleStateCheckerManagerTests()
    {
        var services = new ServiceCollection();
        services.AddCoreRuntime();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task IsEnabled_ReturnsTrue_WhenNoCheckers()
    {
        var manager = _serviceProvider.GetRequiredService<ISimpleStateCheckerManager>();
        var state = new TestState([]);

        var result = await manager.IsEnabledAsync(state, TestContext.Current.CancellationToken);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task IsEnabled_ReturnsFalse_WhenAnyCheckerReturnsFalse()
    {
        var manager = _serviceProvider.GetRequiredService<ISimpleStateCheckerManager>();
        var state = new TestState([new AlwaysTrueChecker(), new AlwaysFalseChecker()]);

        var result = await manager.IsEnabledAsync(state, TestContext.Current.CancellationToken);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task IsEnabled_ReturnsTrue_WhenAllCheckersReturnTrue()
    {
        var manager = _serviceProvider.GetRequiredService<ISimpleStateCheckerManager>();
        var state = new TestState([new AlwaysTrueChecker(), new AlwaysTrueChecker()]);

        var result = await manager.IsEnabledAsync(state, TestContext.Current.CancellationToken);

        result.ShouldBeTrue();
    }

    public void Dispose() => _serviceProvider.Dispose();

    private sealed class TestState(IList<ISimpleStateChecker<IHasSimpleStateCheckers>> checkers) : IHasSimpleStateCheckers
    {
        public IList<ISimpleStateChecker<IHasSimpleStateCheckers>> StateCheckers { get; } = checkers;
    }

    private sealed class AlwaysTrueChecker : ISimpleStateChecker<IHasSimpleStateCheckers>
    {
        public Task<bool> IsEnabledAsync(SimpleStateCheckerContext<IHasSimpleStateCheckers> context, CancellationToken cancellationToken = default)
            => Task.FromResult(true);
    }

    private sealed class AlwaysFalseChecker : ISimpleStateChecker<IHasSimpleStateCheckers>
    {
        public Task<bool> IsEnabledAsync(SimpleStateCheckerContext<IHasSimpleStateCheckers> context, CancellationToken cancellationToken = default)
            => Task.FromResult(false);
    }
}
