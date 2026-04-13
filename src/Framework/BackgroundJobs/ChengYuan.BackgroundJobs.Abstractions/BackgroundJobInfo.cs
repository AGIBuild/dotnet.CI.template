using System;

namespace ChengYuan.BackgroundJobs;

public sealed class BackgroundJobInfo
{
    public required string Id { get; init; }

    public required string JobName { get; init; }

    public required string JobArgs { get; init; }

    public BackgroundJobPriority Priority { get; init; } = BackgroundJobPriority.Normal;

    public int TryCount { get; set; }

    public DateTimeOffset CreationTime { get; init; }

    public DateTimeOffset NextTryTime { get; set; }

    public DateTimeOffset? LastTryTime { get; set; }

    public bool IsAbandoned { get; set; }
}
