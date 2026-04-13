using ChengYuan.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChengYuan.EntityFrameworkCore;

public static class ChengYuanModelBuilderExtensions
{
    public static ModelBuilder ConfigureConcurrencyStamp(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IHasConcurrencyStamp).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(IHasConcurrencyStamp.ConcurrencyStamp))
                .IsConcurrencyToken()
                .HasMaxLength(40);
        }

        return modelBuilder;
    }
}
