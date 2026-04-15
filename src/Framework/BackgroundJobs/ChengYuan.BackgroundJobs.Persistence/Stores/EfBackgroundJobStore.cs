using Microsoft.EntityFrameworkCore;

namespace ChengYuan.BackgroundJobs;

public sealed class EfBackgroundJobStore(IDbContextFactory<BackgroundJobDbContext> dbContextFactory) : IBackgroundJobStore
{
    public async Task InsertAsync(BackgroundJobInfo jobInfo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(jobInfo);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.BackgroundJobs.AddAsync(jobInfo, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<BackgroundJobInfo>> GetWaitingJobsAsync(int maxResultCount, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
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

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.BackgroundJobs.Update(jobInfo);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.BackgroundJobs
            .Where(job => job.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
