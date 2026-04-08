using Microsoft.EntityFrameworkCore;

namespace ChengYuan.Identity;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<IdentityUser> Users => Set<IdentityUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfiguration(new IdentityUserConfiguration());
    }
}
