using Microsoft.EntityFrameworkCore;

namespace ChengYuan.BackgroundJobs;

public sealed class EfBackgroundJobStore(BackgroundJobDbContext dbContext) : IBackgroundJobStore
{
    public async Task InsertAsync(BackgroundJobInfo jobInfo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(jobInfo);

        await dbContext.BackgroundJobs.AddAsync(jobInfo, cancellationToken);
    }

    public async Task<List<BackgroundJobInfo>> GetWaitingJobsAsync(int maxResultCount, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        return await dbContext.BackgroundJobs
            .Where(job => !job.IsAbandoned && job.NextTryTime <= now)
            .OrderBy(job => job.Priority)
            .ThenBy(job => job.NextTryTime)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(BackgroundJobInfo jobInfo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(jobInfo);

        dbContext.BackgroundJobs.Update(jobInfo);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var jobInfo = await dbContext.BackgroundJobs
            .Where(job => job.Id == id)
            .SingleOrDefaultAsync(cancellationToken);

        if (jobInfo is not null)
        {
            dbContext.BackgroundJobs.Remove(jobInfo);
        }
    }
}
