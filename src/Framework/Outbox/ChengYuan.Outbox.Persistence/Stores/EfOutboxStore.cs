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
        await dbContext.SaveChangesAsync(cancellationToken);
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
            .OrderBy(e => e.CreatedAtUtc)
            .Take(maxCount)
            .ToListAsync(cancellationToken);

        return entities.ConvertAll(e => e.ToOutboxMessage());
    }

    public async ValueTask MarkDispatchedAsync(Guid messageId, DateTimeOffset dispatchedAtUtc, CancellationToken cancellationToken = default)
    {
        await dbContext.OutboxMessages
            .Where(e => e.Id == messageId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.Status, OutboxMessageStatus.Dispatched)
                .SetProperty(e => e.AttemptCount, e => e.AttemptCount + 1)
                .SetProperty(e => e.DispatchedAtUtc, dispatchedAtUtc)
                .SetProperty(e => e.LastError, (string?)null),
                cancellationToken);
    }

    public async ValueTask MarkFailedAsync(Guid messageId, string errorMessage, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        await dbContext.OutboxMessages
            .Where(e => e.Id == messageId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.Status, OutboxMessageStatus.Failed)
                .SetProperty(e => e.AttemptCount, e => e.AttemptCount + 1)
                .SetProperty(e => e.LastError, errorMessage),
                cancellationToken);
    }
}
