using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ChengYuan.Outbox;

internal sealed class EfOutboxStore(OutboxDbContext dbContext) : IOutboxStore
{
    public async ValueTask SaveAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        var entity = OutboxMessageEntity.FromOutboxMessage(message);
        await dbContext.OutboxMessages.AddAsync(entity, cancellationToken);
    }

    public async ValueTask<OutboxMessage?> GetAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.OutboxMessages.FindAsync([messageId], cancellationToken);
        return entity?.ToOutboxMessage();
    }

    public async ValueTask<IReadOnlyList<OutboxMessage>> GetPendingAsync(int maxCount, CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.OutboxMessages
            .Where(e => e.Status == OutboxMessageStatus.Pending)
            .OrderBy(e => e.CreatedAtUtcTicks)
            .ThenBy(e => e.Id)
            .Take(maxCount)
            .ToListAsync(cancellationToken);

        return entities
            .Select(e => e.ToOutboxMessage())
            .ToArray();
    }

    public async ValueTask MarkDispatchedAsync(Guid messageId, DateTimeOffset dispatchedAtUtc, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.OutboxMessages.FindAsync([messageId], cancellationToken);
        if (entity is null)
        {
            throw new InvalidOperationException($"Outbox message '{messageId}' was not found.");
        }

        entity.Status = OutboxMessageStatus.Dispatched;
        entity.AttemptCount++;
        entity.DispatchedAtUtc = dispatchedAtUtc;
        entity.LastError = null;
    }

    public async ValueTask MarkFailedAsync(Guid messageId, string errorMessage, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        var entity = await dbContext.OutboxMessages.FindAsync([messageId], cancellationToken);
        if (entity is null)
        {
            throw new InvalidOperationException($"Outbox message '{messageId}' was not found.");
        }

        entity.Status = OutboxMessageStatus.Failed;
        entity.AttemptCount++;
        entity.LastError = errorMessage;
    }
}
