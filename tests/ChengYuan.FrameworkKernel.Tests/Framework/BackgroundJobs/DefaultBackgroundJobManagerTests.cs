using ChengYuan.BackgroundJobs;
using ChengYuan.Core.Timing;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class DefaultBackgroundJobManagerTests
{
    [Fact]
    public async Task EnqueueAsync_CreatesJobInStore()
    {
        var store = new InMemoryBackgroundJobStore();
        var clock = new FakeClock(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var manager = new DefaultBackgroundJobManager(store, clock);

        var jobId = await manager.EnqueueAsync(new TestJobArgs { Name = "test" });

        jobId.ShouldNotBeNullOrWhiteSpace();

        var jobs = await store.GetWaitingJobsAsync(10, TestContext.Current.CancellationToken);
        jobs.ShouldContain(j => j.Id == jobId);
    }

    [Fact]
    public async Task EnqueueAsync_WithDelay_SetsNextTryTimeInFuture()
    {
        var now = DateTimeOffset.UtcNow;
        var store = new InMemoryBackgroundJobStore();
        var clock = new FakeClock(now);
        var manager = new DefaultBackgroundJobManager(store, clock);

        await manager.EnqueueAsync(new TestJobArgs { Name = "delayed" }, delay: TimeSpan.FromHours(1));

        var jobs = await store.GetWaitingJobsAsync(10, TestContext.Current.CancellationToken);
        // Job should NOT appear in waiting list since NextTryTime is 1 hour from now
        jobs.ShouldBeEmpty();
    }

    [Fact]
    public async Task EnqueueAsync_SetsJobName_FromArgsType()
    {
        var store = new InMemoryBackgroundJobStore();
        var clock = new FakeClock(DateTimeOffset.UtcNow);
        var manager = new DefaultBackgroundJobManager(store, clock);

        await manager.EnqueueAsync(new TestJobArgs { Name = "check-type" });

        var jobs = await store.GetWaitingJobsAsync(10, TestContext.Current.CancellationToken);
        jobs.ShouldContain(j => j.JobName == typeof(TestJobArgs).FullName);
    }

    private sealed class FakeClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow => utcNow;
    }
}

public sealed class TestJobArgs
{
    public required string Name { get; init; }
}
