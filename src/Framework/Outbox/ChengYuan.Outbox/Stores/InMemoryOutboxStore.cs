using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Outbox;

internal sealed class InMemoryOutboxStore : IOutboxStore
{
    private readonly ConcurrentDictionary<Guid, OutboxMessage> _messages = new();

    public ValueTask SaveAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _messages[message.Id] = message;
        return ValueTask.CompletedTask;
    }

    public ValueTask<OutboxMessage?> GetAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        _messages.TryGetValue(messageId, out var message);
        return ValueTask.FromResult(message);
    }

    public ValueTask<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxCount, CancellationToken cancellationToken = default)
    {
        var messages = _messages.Values
            .Where(message => message.Status == OutboxMessageStatus.Pending)
            .OrderBy(message => message.CreatedAtUtc)
            .Take(maxCount)
            .ToArray();

        return ValueTask.FromResult<IReadOnlyList<OutboxMessage>>(messages);
    }

    public ValueTask MarkDispatchedAsync(Guid messageId, DateTimeOffset dispatchedAtUtc, CancellationToken cancellationToken = default)
    {
        _messages.AddOrUpdate(
            messageId,
            static (_, __) => throw new InvalidOperationException("Outbox message was not found."),
            static (_, message, timestamp) => message with
            {
                Status = OutboxMessageStatus.Dispatched,
                AttemptCount = message.AttemptCount + 1,
                DispatchedAtUtc = timestamp,
                LastError = null
            },
            dispatchedAtUtc);

        return ValueTask.CompletedTask;
    }

    public ValueTask MarkFailedAsync(Guid messageId, string errorMessage, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        _messages.AddOrUpdate(
            messageId,
            static (_, __) => throw new InvalidOperationException("Outbox message was not found."),
            static (_, message, failure) => message with
            {
                Status = OutboxMessageStatus.Failed,
                AttemptCount = message.AttemptCount + 1,
                LastError = failure
            },
            errorMessage);

        return ValueTask.CompletedTask;
    }
}
