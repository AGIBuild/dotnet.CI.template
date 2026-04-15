using ChengYuan.Core.Data;
using ChengYuan.Core.Data.Auditing;
using ChengYuan.Core.Timing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ChengYuan.EntityFrameworkCore;

public sealed class AuditPropertySetterInterceptor(IAuditUserProvider auditUserProvider, IClock clock) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SetAuditProperties(eventData.Context);
        return new ValueTask<InterceptionResult<int>>(result);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        SetAuditProperties(eventData.Context);
        return result;
    }

    private void SetAuditProperties(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = clock.UtcNow;
        var userId = auditUserProvider.UserId;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    SetCreationProperties(entry, now, userId);
                    break;

                case EntityState.Modified:
                    SetModificationProperties(entry, now, userId);
                    break;
            }
        }
    }

    private static void SetCreationProperties(EntityEntry entry, DateTimeOffset now, string? userId)
    {
        var entityType = entry.Metadata.ClrType;

        if (typeof(IHasCreationTime).IsAssignableFrom(entityType))
        {
            entry.Property(nameof(IHasCreationTime.CreationTime)).CurrentValue = now;
        }

        if (typeof(IHasCreatorId).IsAssignableFrom(entityType) && userId is not null)
        {
            entry.Property(nameof(IHasCreatorId.CreatorId)).CurrentValue = userId;
        }
    }

    private static void SetModificationProperties(EntityEntry entry, DateTimeOffset now, string? userId)
    {
        var entityType = entry.Metadata.ClrType;

        if (typeof(IHasModificationTime).IsAssignableFrom(entityType))
        {
            entry.Property(nameof(IHasModificationTime.LastModificationTime)).CurrentValue = now;
        }

        if (typeof(IHasModifierId).IsAssignableFrom(entityType) && userId is not null)
        {
            entry.Property(nameof(IHasModifierId.LastModifierId)).CurrentValue = userId;
        }
    }
}
