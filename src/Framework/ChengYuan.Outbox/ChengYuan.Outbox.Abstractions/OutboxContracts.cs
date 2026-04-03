namespace ChengYuan.Outbox;

public enum OutboxMessageStatus
{
    Pending = 0,
    Dispatched = 1,
    Failed = 2
}

public sealed record OutboxPayload(byte[] Content, string TypeName);

public sealed record OutboxMessage(
    Guid Id,
    string Name,
    OutboxPayload Payload,
    DateTimeOffset CreatedAtUtc,
    Guid? TenantId,
    string? CorrelationId,
    OutboxMessageStatus Status,
    int AttemptCount,
    DateTimeOffset? DispatchedAtUtc,
    string? LastError);

public sealed record OutboxDrainResult(int AttemptedCount, int DispatchedCount, int FailedCount);

public interface IOutbox
{
    ValueTask<Guid> EnqueueAsync<T>(string name, T payload, CancellationToken cancellationToken = default);
}

public interface IOutboxStore
{
    ValueTask SaveAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    ValueTask<OutboxMessage?> GetAsync(Guid messageId, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxCount, CancellationToken cancellationToken = default);

    ValueTask MarkDispatchedAsync(Guid messageId, DateTimeOffset dispatchedAtUtc, CancellationToken cancellationToken = default);

    ValueTask MarkFailedAsync(Guid messageId, string errorMessage, CancellationToken cancellationToken = default);
}

public interface IOutboxSerializer
{
    OutboxPayload Serialize<T>(T payload);

    T? Deserialize<T>(OutboxPayload payload);
}

public interface IOutboxDispatcher
{
    ValueTask DispatchAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}

public interface IOutboxWorker
{
    ValueTask<OutboxDrainResult> DrainAsync(int maxCount = 100, CancellationToken cancellationToken = default);
}
