using ChengYuan.Core.Data;
using ChengYuan.Core.Entities;
using ChengYuan.Core.StronglyTypedIds;
using ChengYuan.Core.Timing;
using ChengYuan.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class CoreEntityFrameworkCoreTests
{
    [Fact]
    public async Task EfRepository_ShouldInsertAndLoadAggregateRoots()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfRepository<TestDbContext, Workspace, WorkspaceId>(dbContext);
        var workspace = new Workspace(new WorkspaceId(Guid.NewGuid()), "core-runtime");

        await repository.InsertAsync(workspace, TestContext.Current.CancellationToken);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var loadedWorkspace = await repository.GetAsync(workspace.Id, TestContext.Current.CancellationToken);

        loadedWorkspace.Name.ShouldBe("core-runtime");
        loadedWorkspace.Id.ShouldBe(workspace.Id);
    }

    [Fact]
    public async Task EfRepository_ShouldReturnNullWhenAggregateRootIsMissing()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfRepository<TestDbContext, Workspace, WorkspaceId>(dbContext);

        var workspace = await repository.FindAsync(new WorkspaceId(Guid.NewGuid()), TestContext.Current.CancellationToken);

        workspace.ShouldBeNull();
    }

    [Fact]
    public async Task EfRepository_ShouldRemoveAggregateRoots()
    {
        await using var dbContext = CreateDbContext();
        var softDeleteFilter = new DataFilter<SoftDeleteFilter>();
        var repository = new EfRepository<TestDbContext, Workspace, WorkspaceId>(dbContext, softDeleteFilter: softDeleteFilter, clock: new StubClock());
        var workspace = new Workspace(new WorkspaceId(Guid.NewGuid()), "core-data");

        await repository.InsertAsync(workspace, TestContext.Current.CancellationToken);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await repository.DeleteAsync(workspace, TestContext.Current.CancellationToken);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var loadedWorkspace = await repository.FindAsync(workspace.Id, TestContext.Current.CancellationToken);

        loadedWorkspace.ShouldBeNull();
    }

    [Fact]
    public async Task EfRepository_ShouldExposeSoftDeletedAggregateRoots_WhenFilterIsDisabled()
    {
        await using var dbContext = CreateDbContext();
        var softDeleteFilter = new DataFilter<SoftDeleteFilter>();
        var repository = new EfRepository<TestDbContext, Workspace, WorkspaceId>(dbContext, softDeleteFilter: softDeleteFilter, clock: new StubClock());
        var workspace = new Workspace(new WorkspaceId(Guid.NewGuid()), "soft-delete");

        await repository.InsertAsync(workspace, TestContext.Current.CancellationToken);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await repository.DeleteAsync(workspace, TestContext.Current.CancellationToken);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        using (softDeleteFilter.Disable())
        {
            var deletedWorkspace = await repository.GetAsync(workspace.Id, TestContext.Current.CancellationToken);

            deletedWorkspace.IsDeleted.ShouldBeTrue();
            deletedWorkspace.DeletionTime.ShouldBe(new StubClock().UtcNow);
        }
    }

    [Fact]
    public async Task EfRepository_ShouldHideOtherTenantAggregateRoots_WhenMultiTenantFilterIsEnabled()
    {
        await using var dbContext = CreateDbContext();
        var multiTenantFilter = new DataFilter<MultiTenantFilter>();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var repository = new EfRepository<TestDbContext, Workspace, WorkspaceId>(
            dbContext,
            multiTenantFilter: multiTenantFilter,
            dataTenantProvider: new StubDataTenantProvider(tenantA));
        var tenantAWorkspace = new Workspace(new WorkspaceId(Guid.NewGuid()), "tenant-a", tenantA);
        var tenantBWorkspace = new Workspace(new WorkspaceId(Guid.NewGuid()), "tenant-b", tenantB);

        await repository.InsertAsync(tenantAWorkspace, TestContext.Current.CancellationToken);
        await repository.InsertAsync(tenantBWorkspace, TestContext.Current.CancellationToken);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var visibleWorkspace = await repository.FindAsync(tenantAWorkspace.Id, TestContext.Current.CancellationToken);
        var hiddenWorkspace = await repository.FindAsync(tenantBWorkspace.Id, TestContext.Current.CancellationToken);

        visibleWorkspace.ShouldNotBeNull();
        hiddenWorkspace.ShouldBeNull();

        using (multiTenantFilter.Disable())
        {
            var otherTenantWorkspace = await repository.GetAsync(tenantBWorkspace.Id, TestContext.Current.CancellationToken);

            otherTenantWorkspace.Name.ShouldBe("tenant-b");
            otherTenantWorkspace.TenantId.ShouldBe(tenantB);
        }
    }

    [Fact]
    public async Task AddEfRepository_ShouldRegisterRepositoryAndUnitOfWorkServices()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase($"core-ef-di-{Guid.NewGuid():N}"));
        services.AddSingleton<IDomainEventPublisher, NullDomainEventPublisher>();
        services.AddEntityFrameworkCoreDataAccess<TestDbContext>();
        services.AddEfRepository<TestDbContext, Workspace, WorkspaceId>();

        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Workspace, WorkspaceId>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var workspace = new Workspace(new WorkspaceId(Guid.NewGuid()), "di-registration");

        await repository.InsertAsync(workspace, TestContext.Current.CancellationToken);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var loadedWorkspace = await repository.GetAsync(workspace.Id, TestContext.Current.CancellationToken);

        loadedWorkspace.Name.ShouldBe("di-registration");
    }

    private static TestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase($"core-ef-{Guid.NewGuid():N}")
            .Options;

        return new TestDbContext(options);
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<Workspace> Workspaces => Set<Workspace>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Workspace>(builder =>
            {
                builder.HasKey(workspace => workspace.Id);
                builder.Property(workspace => workspace.Id)
                    .HasConversion(new StronglyTypedIdValueConverter<WorkspaceId, Guid>());
                builder.Property(workspace => workspace.IsDeleted);
                builder.Property(workspace => workspace.DeletionTime);
                builder.Property(workspace => workspace.TenantId);
            });
        }
    }

    private sealed class Workspace : AggregateRoot<WorkspaceId>, ISoftDelete, IHasDeletionTime, IMultiTenant
    {
        private Workspace()
        {
            Name = string.Empty;
        }

        public Workspace(WorkspaceId id, string name, Guid? tenantId = null)
            : base(id)
        {
            Name = name;
            TenantId = tenantId;
        }

        public string Name { get; private set; }

        public Guid? TenantId { get; private set; }

        public bool IsDeleted { get; private set; }

        public DateTimeOffset? DeletionTime { get; private set; }
    }

    private sealed record WorkspaceId : GuidStronglyTypedId
    {
        public WorkspaceId(Guid value)
            : base(value)
        {
        }
    }

    private sealed class StubClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 4, 3, 0, 0, 0, TimeSpan.Zero);
    }

    private sealed class StubDataTenantProvider(Guid? tenantId) : IDataTenantProvider
    {
        public Guid? TenantId { get; } = tenantId;

        public bool IsAvailable => TenantId.HasValue;
    }
}
