using System;
using System.Collections.Generic;

namespace ChengYuan.BackgroundJobs;

public sealed class BackgroundJobOptions
{
    private readonly Dictionary<string, Type> _jobTypeMap = new(StringComparer.Ordinal);

    public int MaxJobFetchCount { get; set; } = 100;

    public int DefaultFirstWaitDuration { get; set; } = 60;

    public double DefaultWaitFactor { get; set; } = 2.0;

    public int MaxTryCount { get; set; } = 10;

    public void AddJob<TArgs, TJob>()
        where TJob : IBackgroundJob<TArgs>
    {
        _jobTypeMap[typeof(TArgs).FullName!] = typeof(TJob);
    }

    internal Type? GetJobType(string jobName)
    {
        return _jobTypeMap.GetValueOrDefault(jobName);
    }
}
