using ChengYuan.DistributedLocking;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class LocalDistributedLockTests
{
    [Fact]
    public async Task TryAcquireAsync_ReturnsHandle_WhenAvailable()
    {
        var lockProvider = new LocalDistributedLock();

        await using var handle = await lockProvider.TryAcquireAsync("test-lock", TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        handle.ShouldNotBeNull();
    }

    [Fact]
    public async Task TryAcquireAsync_ReturnsNull_WhenAlreadyHeld_AndTimeoutExpires()
    {
        var lockProvider = new LocalDistributedLock();

        await using var handle1 = await lockProvider.TryAcquireAsync("exclusive-lock", TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        handle1.ShouldNotBeNull();

        var handle2 = await lockProvider.TryAcquireAsync("exclusive-lock", TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);

        handle2.ShouldBeNull();
    }

    [Fact]
    public async Task TryAcquireAsync_Succeeds_AfterPreviousLockReleased()
    {
        var lockProvider = new LocalDistributedLock();

        var handle1 = await lockProvider.TryAcquireAsync("reuse-lock", TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        handle1.ShouldNotBeNull();
        await handle1.DisposeAsync();

        await using var handle2 = await lockProvider.TryAcquireAsync("reuse-lock", TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        handle2.ShouldNotBeNull();
    }

    [Fact]
    public async Task TryAcquireAsync_DifferentNames_CanBeHeldConcurrently()
    {
        var lockProvider = new LocalDistributedLock();

        await using var handle1 = await lockProvider.TryAcquireAsync("lock-a", TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        await using var handle2 = await lockProvider.TryAcquireAsync("lock-b", TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        handle1.ShouldNotBeNull();
        handle2.ShouldNotBeNull();
    }
}
