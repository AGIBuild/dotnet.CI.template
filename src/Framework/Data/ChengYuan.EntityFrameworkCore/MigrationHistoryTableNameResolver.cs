using Microsoft.EntityFrameworkCore;

namespace ChengYuan.EntityFrameworkCore;

/// <summary>
/// Resolves the migration history table name for a given DbContext type.
/// Each module gets an isolated table: <c>__EFMigrationsHistory_{ModuleName}</c>.
/// </summary>
public static class MigrationHistoryTableNameResolver
{
    private const string Prefix = "__EFMigrationsHistory_";

    public static string Resolve<TDbContext>() where TDbContext : DbContext
        => Resolve(typeof(TDbContext));

    public static string Resolve(Type dbContextType)
    {
        var name = dbContextType.Name;

        if (name.EndsWith("DbContext", StringComparison.Ordinal))
        {
            name = name[..^"DbContext".Length];
        }

        return Prefix + name;
    }
}
