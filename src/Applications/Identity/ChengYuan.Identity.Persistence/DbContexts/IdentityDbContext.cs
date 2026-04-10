using Microsoft.EntityFrameworkCore;

namespace ChengYuan.Identity;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<IdentityRole> Roles => Set<IdentityRole>();

    public DbSet<IdentityUser> Users => Set<IdentityUser>();

    public DbSet<IdentityUserRole> UserRoles => Set<IdentityUserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfiguration(new IdentityRoleConfiguration());
        modelBuilder.ApplyConfiguration(new IdentityUserConfiguration());
        modelBuilder.ApplyConfiguration(new IdentityUserRoleConfiguration());
    }
}
