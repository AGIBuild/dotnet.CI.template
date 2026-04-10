using System;

namespace ChengYuan.Outbox;

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
