using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.BackgroundWorkers;
using ChengYuan.Core.Timing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChengYuan.BackgroundJobs;

public sealed partial class BackgroundJobExecutionWorker : AsyncPeriodicBackgroundWorkerBase
{
    public BackgroundJobExecutionWorker(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<BackgroundJobExecutionWorker> logger)
        : base(TimeSpan.FromSeconds(5), serviceScopeFactory, logger)
    {
    }

    protected override async Task DoWorkAsync(BackgroundWorkerContext workerContext)
    {
        var store = workerContext.ServiceProvider.GetRequiredService<IBackgroundJobStore>();
        var options = workerContext.ServiceProvider.GetRequiredService<IOptions<BackgroundJobOptions>>().Value;
        var clock = workerContext.ServiceProvider.GetRequiredService<IClock>();

        var waitingJobs = await store.GetWaitingJobsAsync(options.MaxJobFetchCount, workerContext.CancellationToken);

        foreach (var jobInfo in waitingJobs)
        {
            try
            {
                var jobType = options.GetJobType(jobInfo.JobName);
                if (jobType is null)
                {
                    LogJobTypeNotFound(Logger, jobInfo.JobName);
                    jobInfo.IsAbandoned = true;
                    await store.UpdateAsync(jobInfo, workerContext.CancellationToken);
                    continue;
                }

                var job = workerContext.ServiceProvider.GetRequiredService(jobType);
                var argsType = jobType.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBackgroundJob<>))
                    .GetGenericArguments()[0];
                var args = JsonSerializer.Deserialize(jobInfo.JobArgs, argsType)!;

                var executeMethod = jobType.GetMethod(nameof(IBackgroundJob<object>.ExecuteAsync))!;
                var task = (Task)executeMethod.Invoke(job, [args, workerContext.CancellationToken])!;
                await task;

                await store.DeleteAsync(jobInfo.Id, workerContext.CancellationToken);
            }
            catch (Exception ex)
            {
                jobInfo.TryCount++;
                jobInfo.LastTryTime = clock.UtcNow;

                if (jobInfo.TryCount >= options.MaxTryCount)
                {
                    jobInfo.IsAbandoned = true;
                    LogJobAbandoned(Logger, jobInfo.JobName, jobInfo.Id, ex);
                }
                else
                {
                    var nextWait = options.DefaultFirstWaitDuration * Math.Pow(options.DefaultWaitFactor, jobInfo.TryCount - 1);
                    jobInfo.NextTryTime = clock.UtcNow.AddSeconds(nextWait);
                    LogJobFailed(Logger, jobInfo.JobName, jobInfo.Id, jobInfo.TryCount, ex);
                }

                await store.UpdateAsync(jobInfo, workerContext.CancellationToken);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Background job type not found for '{JobName}'.")]
    private static partial void LogJobTypeNotFound(ILogger logger, string jobName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Background job '{JobName}' (id: {JobId}) has been abandoned after max retries.")]
    private static partial void LogJobAbandoned(ILogger logger, string jobName, string jobId, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Background job '{JobName}' (id: {JobId}) failed, try count: {TryCount}. Will retry.")]
    private static partial void LogJobFailed(ILogger logger, string jobName, string jobId, int tryCount, Exception exception);
}
