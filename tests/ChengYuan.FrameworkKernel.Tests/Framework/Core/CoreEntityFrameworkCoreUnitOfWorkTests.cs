using ChengYuan.Core.Data;
using ChengYuan.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class CoreEntityFrameworkCoreUnitOfWorkTests
{
    [Fact]
    public async Task AddEntityFrameworkCoreDataAccess_ShouldCommitAllRegisteredDbContextsInScope()
    {
        var databaseName = $"core-ef-uow-{Guid.NewGuid():N}";
        var services = new ServiceCollection();

        services.AddDbContext<LeftTestDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddDbContext<RightTestDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddEntityFrameworkCoreDataAccess<LeftTestDbContext>();
        services.AddEntityFrameworkCoreDataAccess<RightTestDbContext>();

        await using var serviceProvider = services.BuildServiceProvider();

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var leftDbContext = scope.ServiceProvider.GetRequiredService<LeftTestDbContext>();
            var rightDbContext = scope.ServiceProvider.GetRequiredService<RightTestDbContext>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await leftDbContext.LeftRecords.AddAsync(new LeftRecord(Guid.NewGuid(), "left"), TestContext.Current.CancellationToken);
            await rightDbContext.RightRecords.AddAsync(new RightRecord(Guid.NewGuid(), "right"), TestContext.Current.CancellationToken);

            await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var verificationScope = serviceProvider.CreateAsyncScope())
        {
            var leftDbContext = verificationScope.ServiceProvider.GetRequiredService<LeftTestDbContext>();
            var rightDbContext = verificationScope.ServiceProvider.GetRequiredService<RightTestDbContext>();

            var leftRecords = await leftDbContext.LeftRecords.OrderBy(record => record.Name).ToArrayAsync(TestContext.Current.CancellationToken);
            var rightRecords = await rightDbContext.RightRecords.OrderBy(record => record.Name).ToArrayAsync(TestContext.Current.CancellationToken);

            leftRecords.Select(record => record.Name).ShouldBe(["left"]);
            rightRecords.Select(record => record.Name).ShouldBe(["right"]);
        }
    }

    private sealed class LeftTestDbContext(DbContextOptions<LeftTestDbContext> options) : DbContext(options)
    {
        public DbSet<LeftRecord> LeftRecords => Set<LeftRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LeftRecord>(builder =>
            {
                builder.HasKey(record => record.Id);
                builder.Property(record => record.Name)
                    .HasMaxLength(128)
                    .IsRequired();
            });
        }
    }

    private sealed class RightTestDbContext(DbContextOptions<RightTestDbContext> options) : DbContext(options)
    {
        public DbSet<RightRecord> RightRecords => Set<RightRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RightRecord>(builder =>
            {
                builder.HasKey(record => record.Id);
                builder.Property(record => record.Name)
                    .HasMaxLength(128)
                    .IsRequired();
            });
        }
    }

    private sealed record LeftRecord(Guid Id, string Name);

    private sealed record RightRecord(Guid Id, string Name);
}
