using ChengYuan.Core.Data.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ChengYuan.EntityFrameworkCore;

public sealed class AuditEntityChangeInterceptor(IEntityChangeCollector collector) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CollectChanges(eventData.Context);
        return new ValueTask<InterceptionResult<int>>(result);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        CollectChanges(eventData.Context);
        return result;
    }

    private void CollectChanges(DbContext? context)
    {
        if (context is null || !collector.IsActive)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            var changeInfo = new EntityChangeInfo
            {
                EntityTypeFullName = entry.Metadata.ClrType.FullName!,
                EntityId = GetPrimaryKeyValue(entry),
                ChangeType = entry.State switch
                {
                    EntityState.Added => EntityChangeType.Created,
                    EntityState.Modified => EntityChangeType.Updated,
                    EntityState.Deleted => EntityChangeType.Deleted,
                    _ => EntityChangeType.Updated,
                },
                PropertyChanges = CollectPropertyChanges(entry),
            };

            collector.Collect(changeInfo);
        }
    }

    private static string? GetPrimaryKeyValue(EntityEntry entry)
    {
        var keyProperties = entry.Metadata.FindPrimaryKey()?.Properties;
        if (keyProperties is null or { Count: 0 })
        {
            return null;
        }

        return keyProperties.Count == 1
            ? entry.Property(keyProperties[0].Name).CurrentValue?.ToString()
            : string.Join(',', keyProperties.Select(p => entry.Property(p.Name).CurrentValue));
    }

    private static List<PropertyChangeInfo> CollectPropertyChanges(EntityEntry entry)
    {
        List<PropertyChangeInfo> changes = [];

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsPrimaryKey())
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    changes.Add(new PropertyChangeInfo(property.Metadata.Name, null, property.CurrentValue));
                    break;

                case EntityState.Deleted:
                    changes.Add(new PropertyChangeInfo(property.Metadata.Name, property.OriginalValue, null));
                    break;

                case EntityState.Modified when property.IsModified:
                    changes.Add(new PropertyChangeInfo(property.Metadata.Name, property.OriginalValue, property.CurrentValue));
                    break;
            }
        }

        return changes;
    }
}
