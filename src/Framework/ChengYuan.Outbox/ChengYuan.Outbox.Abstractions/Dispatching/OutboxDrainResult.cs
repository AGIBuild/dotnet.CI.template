namespace ChengYuan.Outbox;

public sealed record OutboxDrainResult(int AttemptedCount, int DispatchedCount, int FailedCount);
