using ChengYuan.Core.Data;
using ChengYuan.Core.Data.Auditing;
using ChengYuan.Core.Timing;
using ChengYuan.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class AuditPropertySetterInterceptorTests
{
    [Fact]
    public async Task SavingChanges_ShouldSetCreationTime_OnAddedEntityWithIHasCreationTime()
    {
        var fixedTime = new DateTimeOffset(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);
        var clock = new FixedClock(fixedTime);
        var interceptor = new AuditPropertySetterInterceptor(new NullAuditUserProvider(), clock);

        using var dbContext = CreateDbContext(interceptor);
        var entity = new AuditedTestEntity { Id = Guid.NewGuid(), Name = "Test" };
        dbContext.AuditedEntities.Add(entity);

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        entity.CreationTime.ShouldBe(fixedTime);
    }

    [Fact]
    public async Task SavingChanges_ShouldSetCreatorId_OnAddedEntityWithIHasCreatorId()
    {
        var clock = new FixedClock(DateTimeOffset.UtcNow);
        var userProvider = new TestAuditUserProvider("user-42");
        var interceptor = new AuditPropertySetterInterceptor(userProvider, clock);

        using var dbContext = CreateDbContext(interceptor);
        var entity = new AuditedTestEntity { Id = Guid.NewGuid(), Name = "Test" };
        dbContext.AuditedEntities.Add(entity);

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        entity.CreatorId.ShouldBe("user-42");
    }

    [Fact]
    public async Task SavingChanges_ShouldSetModificationTime_OnModifiedEntityWithIHasModificationTime()
    {
        var fixedTime = new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);
        var clock = new FixedClock(fixedTime);
        var interceptor = new AuditPropertySetterInterceptor(new NullAuditUserProvider(), clock);

        using var dbContext = CreateDbContext(interceptor);
        var entity = new AuditedTestEntity { Id = Guid.NewGuid(), Name = "Original" };
        dbContext.AuditedEntities.Add(entity);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        entity.Name = "Updated";
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        entity.LastModificationTime.ShouldBe(fixedTime);
    }

    [Fact]
    public async Task SavingChanges_ShouldSetModifierId_OnModifiedEntityWithIHasModifierId()
    {
        var clock = new FixedClock(DateTimeOffset.UtcNow);
        var userProvider = new TestAuditUserProvider("admin-1");
        var interceptor = new AuditPropertySetterInterceptor(userProvider, clock);

        using var dbContext = CreateDbContext(interceptor);
        var entity = new AuditedTestEntity { Id = Guid.NewGuid(), Name = "Original" };
        dbContext.AuditedEntities.Add(entity);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        entity.Name = "Updated";
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        entity.LastModifierId.ShouldBe("admin-1");
    }

    private static AuditPropertySetterTestDbContext CreateDbContext(params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<AuditPropertySetterTestDbContext>()
            .UseInMemoryDatabase($"audit-prop-setter-{Guid.NewGuid():N}")
            .AddInterceptors(interceptors)
            .Options;

        return new AuditPropertySetterTestDbContext(options);
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow => utcNow;
    }

    private sealed class TestAuditUserProvider(string? userId) : IAuditUserProvider
    {
        public string? UserId => userId;
    }

    internal sealed class AuditPropertySetterTestDbContext(DbContextOptions<AuditPropertySetterTestDbContext> options)
        : DbContext(options)
    {
        public DbSet<AuditedTestEntity> AuditedEntities => Set<AuditedTestEntity>();
    }

    internal sealed class AuditedTestEntity : IAudited
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTimeOffset CreationTime { get; set; }

        public string? CreatorId { get; set; }

        public DateTimeOffset? LastModificationTime { get; set; }

        public string? LastModifierId { get; set; }
    }
}
