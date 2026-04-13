using System;
using System.Text.Json;
using System.Threading.Tasks;
using ChengYuan.Core.Timing;

namespace ChengYuan.BackgroundJobs;

public sealed class DefaultBackgroundJobManager(
    IBackgroundJobStore store,
    IClock clock) : IBackgroundJobManager
{
    public async Task<string> EnqueueAsync<TArgs>(TArgs args, BackgroundJobPriority priority = BackgroundJobPriority.Normal, TimeSpan? delay = null)
    {
        ArgumentNullException.ThrowIfNull(args);

        var now = clock.UtcNow;
        var jobInfo = new BackgroundJobInfo
        {
            Id = Guid.NewGuid().ToString("N"),
            JobName = typeof(TArgs).FullName!,
            JobArgs = JsonSerializer.Serialize(args),
            Priority = priority,
            CreationTime = now,
            NextTryTime = delay.HasValue ? now.Add(delay.Value) : now,
        };

        await store.InsertAsync(jobInfo);

        return jobInfo.Id;
    }
}
