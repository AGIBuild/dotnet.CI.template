using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.BackgroundJobs;

public interface IBackgroundJobStore
{
    Task InsertAsync(BackgroundJobInfo jobInfo, CancellationToken cancellationToken = default);

    Task<List<BackgroundJobInfo>> GetWaitingJobsAsync(int maxResultCount, CancellationToken cancellationToken = default);

    Task UpdateAsync(BackgroundJobInfo jobInfo, CancellationToken cancellationToken = default);

    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
