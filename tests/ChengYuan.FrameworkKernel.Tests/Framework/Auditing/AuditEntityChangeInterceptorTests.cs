using ChengYuan.Auditing;
using ChengYuan.Core.Data;
using ChengYuan.Core.Data.Auditing;
using ChengYuan.Core.Timing;
using ChengYuan.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class AuditEntityChangeInterceptorTests
{
    [Fact]
    public async Task SavingChanges_ShouldCollectChanges_ForAuditedEntity()
    {
        var collector = new TestEntityChangeCollector();
        var resolver = CreateResolver();
        var clock = new FixedClock(DateTimeOffset.UtcNow);
        var interceptor = new AuditEntityChangeInterceptor(collector, resolver, clock);

        using var dbContext = CreateDbContext(interceptor);
        dbContext.TrackedEntities.Add(new TrackedEntity { Id = Guid.NewGuid(), Value = "hello" });

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        collector.CollectedChanges.Count.ShouldBe(1);
        collector.CollectedChanges[0].ChangeType.ShouldBe(EntityChangeType.Created);
        collector.CollectedChanges[0].EntityTypeFullName.ShouldContain(nameof(TrackedEntity));
    }

    [Fact]
    public async Task SavingChanges_ShouldSkipNonAuditedEntity()
    {
        var collector = new TestEntityChangeCollector();
        var resolver = CreateResolver();
        var clock = new FixedClock(DateTimeOffset.UtcNow);
        var interceptor = new AuditEntityChangeInterceptor(collector, resolver, clock);

        using var dbContext = CreateDbContext(interceptor);
        dbContext.UntrackedEntities.Add(new UntrackedEntity { Id = Guid.NewGuid(), Data = "ignored" });

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        collector.CollectedChanges.ShouldBeEmpty();
    }

    [Fact]
    public async Task SavingChanges_ShouldSkipPropertiesWithDisableAuditingAttribute()
    {
        var collector = new TestEntityChangeCollector();
        var resolver = CreateResolver();
        var clock = new FixedClock(DateTimeOffset.UtcNow);
        var interceptor = new AuditEntityChangeInterceptor(collector, resolver, clock);

        using var dbContext = CreateDbContext(interceptor);
        dbContext.TrackedEntities.Add(new TrackedEntity { Id = Guid.NewGuid(), Value = "visible", Secret = "hidden" });

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        collector.CollectedChanges.Count.ShouldBe(1);
        var propertyNames = collector.CollectedChanges[0].PropertyChanges.Select(p => p.PropertyName).ToArray();
        propertyNames.ShouldContain("Value");
        propertyNames.ShouldNotContain("Secret");
    }

    [Fact]
    public async Task SavingChanges_ShouldNotCollect_WhenCollectorIsInactive()
    {
        var collector = new TestEntityChangeCollector { Active = false };
        var resolver = CreateResolver();
        var clock = new FixedClock(DateTimeOffset.UtcNow);
        var interceptor = new AuditEntityChangeInterceptor(collector, resolver, clock);

        using var dbContext = CreateDbContext(interceptor);
        dbContext.TrackedEntities.Add(new TrackedEntity { Id = Guid.NewGuid(), Value = "test" });

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        collector.CollectedChanges.ShouldBeEmpty();
    }

    [Fact]
    public async Task SavingChanges_ShouldRecordChangeTime()
    {
        var fixedTime = new DateTimeOffset(2026, 4, 14, 8, 0, 0, TimeSpan.Zero);
        var collector = new TestEntityChangeCollector();
        var resolver = CreateResolver();
        var clock = new FixedClock(fixedTime);
        var interceptor = new AuditEntityChangeInterceptor(collector, resolver, clock);

        using var dbContext = CreateDbContext(interceptor);
        dbContext.TrackedEntities.Add(new TrackedEntity { Id = Guid.NewGuid(), Value = "timed" });

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        collector.CollectedChanges[0].ChangeTime.ShouldBe(fixedTime);
    }

    private static AuditableEntityTypeResolver CreateResolver()
    {
        var options = new AuditingOptions();
        return new AuditableEntityTypeResolver(Options.Create(options));
    }

    private static ChangeInterceptorTestDbContext CreateDbContext(params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<ChangeInterceptorTestDbContext>()
            .UseInMemoryDatabase($"audit-change-{Guid.NewGuid():N}")
            .AddInterceptors(interceptors)
            .Options;

        return new ChangeInterceptorTestDbContext(options);
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow => utcNow;
    }

    private sealed class TestEntityChangeCollector : IEntityChangeCollector
    {
        public bool Active { get; set; } = true;

        public bool IsActive => Active;

        public List<EntityChangeInfo> CollectedChanges { get; } = [];

        public void Collect(EntityChangeInfo changeInfo) => CollectedChanges.Add(changeInfo);
    }

    internal sealed class ChangeInterceptorTestDbContext(DbContextOptions<ChangeInterceptorTestDbContext> options)
        : DbContext(options)
    {
        public DbSet<TrackedEntity> TrackedEntities => Set<TrackedEntity>();

        public DbSet<UntrackedEntity> UntrackedEntities => Set<UntrackedEntity>();
    }

    [Audited]
    internal sealed class TrackedEntity
    {
        public Guid Id { get; set; }

        public string Value { get; set; } = string.Empty;

        [DisableAuditing]
        public string? Secret { get; set; }
    }

    internal sealed class UntrackedEntity
    {
        public Guid Id { get; set; }

        public string Data { get; set; } = string.Empty;
    }
}
