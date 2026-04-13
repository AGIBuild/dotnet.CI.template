using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.BackgroundJobs;

public sealed class InMemoryBackgroundJobStore : IBackgroundJobStore
{
    private readonly ConcurrentDictionary<string, BackgroundJobInfo> _jobs = new(StringComparer.Ordinal);

    public Task InsertAsync(BackgroundJobInfo jobInfo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(jobInfo);

        _jobs[jobInfo.Id] = jobInfo;
        return Task.CompletedTask;
    }

    public Task<List<BackgroundJobInfo>> GetWaitingJobsAsync(int maxResultCount, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var result = _jobs.Values
            .Where(j => !j.IsAbandoned && j.NextTryTime <= now)
            .OrderBy(j => j.Priority)
            .ThenBy(j => j.NextTryTime)
            .Take(maxResultCount)
            .ToList();

        return Task.FromResult(result);
    }

    public Task UpdateAsync(BackgroundJobInfo jobInfo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(jobInfo);

        _jobs[jobInfo.Id] = jobInfo;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _jobs.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
