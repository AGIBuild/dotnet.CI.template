using ChengYuan.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class LocalEventBusTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ILocalEventBus _eventBus;

    public LocalEventBusTests()
    {
        var services = new ServiceCollection();
        services.AddLocalEventBus();
        services.AddEventSubscriber<TestEvent, TestEventSubscriber>();
        _serviceProvider = services.BuildServiceProvider();
        _eventBus = _serviceProvider.GetRequiredService<ILocalEventBus>();
    }

    [Fact]
    public async Task PublishAsync_InvokesSubscriber()
    {
        TestEventSubscriber.Reset();
        var evt = new TestEvent("hello");

        await _eventBus.PublishAsync(evt, TestContext.Current.CancellationToken);

        TestEventSubscriber.ReceivedMessages.ShouldContain("hello");
    }

    [Fact]
    public async Task PublishAsync_ByType_InvokesSubscriber()
    {
        TestEventSubscriber.Reset();
        var evt = new TestEvent("typed");

#pragma warning disable CA2263 // Intentionally testing the non-generic overload
        await _eventBus.PublishAsync(typeof(TestEvent), evt, TestContext.Current.CancellationToken);
#pragma warning restore CA2263

        TestEventSubscriber.ReceivedMessages.ShouldContain("typed");
    }

    [Fact]
    public async Task PublishAsync_WithNoSubscribers_Succeeds()
    {
        var services = new ServiceCollection();
        services.AddLocalEventBus();
        using var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<ILocalEventBus>();

        await Should.NotThrowAsync(() => bus.PublishAsync(new TestEvent("orphan"), TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task PublishAsync_SubscriberThrows_WrapsInAggregateException()
    {
        var services = new ServiceCollection();
        services.AddLocalEventBus();
        services.AddEventSubscriber<TestEvent, FailingEventSubscriber>();
        using var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<ILocalEventBus>();

        var ex = await Should.ThrowAsync<AggregateException>(
            () => bus.PublishAsync(new TestEvent("boom"), TestContext.Current.CancellationToken).AsTask());

        ex.InnerExceptions.ShouldContain(e => e.Message == "Boom!");
    }

    public void Dispose() => _serviceProvider.Dispose();
}

public sealed record TestEvent(string Message);

public sealed class TestEventSubscriber : IEventSubscriber<TestEvent>
{
    private static readonly List<string> Messages = [];

    public static IReadOnlyList<string> ReceivedMessages => Messages;

    public static void Reset() => Messages.Clear();

    public ValueTask HandleAsync(TestEvent eventData, CancellationToken cancellationToken = default)
    {
        Messages.Add(eventData.Message);
        return ValueTask.CompletedTask;
    }
}

public sealed class FailingEventSubscriber : IEventSubscriber<TestEvent>
{
    public ValueTask HandleAsync(TestEvent eventData, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Boom!");
    }
}
