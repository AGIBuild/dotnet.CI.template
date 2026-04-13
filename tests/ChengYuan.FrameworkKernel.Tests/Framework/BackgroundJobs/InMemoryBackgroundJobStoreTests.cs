using ChengYuan.BackgroundJobs;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class InMemoryBackgroundJobStoreTests
{
    [Fact]
    public async Task InsertAsync_ThenGetWaitingJobs_ReturnsJob()
    {
        var store = new InMemoryBackgroundJobStore();
        var jobInfo = CreateJob("TestJob", nextTryTime: DateTimeOffset.UtcNow.AddMinutes(-1));

        await store.InsertAsync(jobInfo, TestContext.Current.CancellationToken);

        var waiting = await store.GetWaitingJobsAsync(10, TestContext.Current.CancellationToken);
        waiting.ShouldContain(j => j.Id == jobInfo.Id);
    }

    [Fact]
    public async Task GetWaitingJobs_ExcludesFutureJobs()
    {
        var store = new InMemoryBackgroundJobStore();
        var futureJob = CreateJob("FutureJob", nextTryTime: DateTimeOffset.UtcNow.AddHours(1));

        await store.InsertAsync(futureJob, TestContext.Current.CancellationToken);

        var waiting = await store.GetWaitingJobsAsync(10, TestContext.Current.CancellationToken);
        waiting.ShouldNotContain(j => j.Id == futureJob.Id);
    }

    [Fact]
    public async Task GetWaitingJobs_ExcludesAbandonedJobs()
    {
        var store = new InMemoryBackgroundJobStore();
        var abandoned = CreateJob("Abandoned", nextTryTime: DateTimeOffset.UtcNow.AddMinutes(-1));
        abandoned.IsAbandoned = true;

        await store.InsertAsync(abandoned, TestContext.Current.CancellationToken);

        var waiting = await store.GetWaitingJobsAsync(10, TestContext.Current.CancellationToken);
        waiting.ShouldBeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_ModifiesExistingJob()
    {
        var store = new InMemoryBackgroundJobStore();
        var job = CreateJob("UpdateTest", nextTryTime: DateTimeOffset.UtcNow.AddMinutes(-1));

        await store.InsertAsync(job, TestContext.Current.CancellationToken);

        job.TryCount = 3;
        await store.UpdateAsync(job, TestContext.Current.CancellationToken);

        var waiting = await store.GetWaitingJobsAsync(10, TestContext.Current.CancellationToken);
        waiting.ShouldContain(j => j.TryCount == 3);
    }

    private static BackgroundJobInfo CreateJob(string name, DateTimeOffset nextTryTime)
    {
        return new BackgroundJobInfo
        {
            Id = Guid.NewGuid().ToString("N"),
            JobName = name,
            JobArgs = "{}",
            CreationTime = DateTimeOffset.UtcNow,
            NextTryTime = nextTryTime,
        };
    }
}
