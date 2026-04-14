using System;
using System.Threading.Tasks;

namespace ChengYuan.BackgroundJobs;

public interface IBackgroundJobManager
{
    Task<string> EnqueueAsync<TArgs>(TArgs args, BackgroundJobPriority priority = BackgroundJobPriority.Normal, TimeSpan? delay = null);
}
