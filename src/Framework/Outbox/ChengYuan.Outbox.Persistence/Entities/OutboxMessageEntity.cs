using System;

namespace ChengYuan.Outbox;

public sealed class OutboxMessageEntity
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required byte[] PayloadContent { get; set; }

    public required string PayloadTypeName { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public Guid? TenantId { get; set; }

    public string? CorrelationId { get; set; }

    public OutboxMessageStatus Status { get; set; }

    public int AttemptCount { get; set; }

    public DateTimeOffset? DispatchedAtUtc { get; set; }

    public string? LastError { get; set; }

    public OutboxMessage ToOutboxMessage()
    {
        return new OutboxMessage(
            Id,
            Name,
            new OutboxPayload(PayloadContent, PayloadTypeName),
            CreatedAtUtc,
            TenantId,
            CorrelationId,
            Status,
            AttemptCount,
            DispatchedAtUtc,
            LastError);
    }

    public static OutboxMessageEntity FromOutboxMessage(OutboxMessage message)
    {
        return new OutboxMessageEntity
        {
            Id = message.Id,
            Name = message.Name,
            PayloadContent = message.Payload.Content,
            PayloadTypeName = message.Payload.TypeName,
            CreatedAtUtc = message.CreatedAtUtc,
            TenantId = message.TenantId,
            CorrelationId = message.CorrelationId,
            Status = message.Status,
            AttemptCount = message.AttemptCount,
            DispatchedAtUtc = message.DispatchedAtUtc,
            LastError = message.LastError,
        };
    }
}
