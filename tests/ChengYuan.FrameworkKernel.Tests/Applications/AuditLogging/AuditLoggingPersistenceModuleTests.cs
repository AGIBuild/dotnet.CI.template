using ChengYuan.Auditing;
using ChengYuan.AuditLogging;
using ChengYuan.Core.Modularity;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class AuditLoggingPersistenceModuleTests
{
    [Fact]
    public void AuditLoggingPersistenceModule_ShouldLoadIntoTheServiceCollection()
    {
        var services = CreateServices();

        using var serviceProvider = services.BuildServiceProvider();

        var moduleCatalog = serviceProvider.GetRequiredService<ModuleCatalog>();
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(AuditLoggingPersistenceModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(AuditLoggingModule));

        using var scope = serviceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<AuditLoggingDbContext>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IAuditLogStore>().ShouldBeOfType<AuditLogStore>();
        serviceProvider.GetRequiredService<IAuditLogReader>().ShouldBeOfType<AuditLogStore>();
        serviceProvider.GetRequiredService<IAuditScopeFactory>().ShouldNotBeNull();
    }

    [Fact]
    public async Task AuditLoggingPersistence_ShouldPersistAuditEntriesProducedByAuditing()
    {
        var services = CreateServices();
        var tenantId = Guid.NewGuid();

        await using var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IAuditScopeFactory>();
        var auditLogReader = serviceProvider.GetRequiredService<IAuditLogReader>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();
        var currentCorrelation = serviceProvider.GetRequiredService<ICurrentCorrelationAccessor>();

        using (currentTenant.Change(tenantId, "tenant-a"))
        using (currentUser.Change(new CurrentUserInfo("alice", "Alice", true)))
        using (currentCorrelation.Change("corr-456"))
        {
            await scopeFactory.ExecuteAsync("workspace.audit.export", async _ =>
            {
                await Task.Yield();
            }, TestContext.Current.CancellationToken);
        }

        var records = await auditLogReader.GetListAsync(TestContext.Current.CancellationToken);
        records.Count.ShouldBe(1);

        var record = records.Single();
        record.Name.ShouldBe("workspace.audit.export");
        record.TenantId.ShouldBe(tenantId);
        record.UserId.ShouldBe("alice");
        record.UserName.ShouldBe("Alice");
        record.CorrelationId.ShouldBe("corr-456");
        record.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public async Task AuditLoggingPersistence_ShouldPersistPropertiesAndFailures()
    {
        var services = CreateServices();

        await using var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IAuditScopeFactory>();
        var auditLogReader = serviceProvider.GetRequiredService<IAuditLogReader>();

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await scopeFactory.ExecuteAsync(
                "workspace.audit.delete",
                _ => ValueTask.FromException(new InvalidOperationException("boom")),
                TestContext.Current.CancellationToken));

        var records = await auditLogReader.GetListAsync(TestContext.Current.CancellationToken);
        var record = records.Single();
        record.Succeeded.ShouldBeFalse();
        record.ErrorMessage.ShouldBe("boom");
        record.TryGetProperty("exceptionType", out var exceptionType).ShouldBeTrue();
        exceptionType.ShouldBe(typeof(InvalidOperationException).FullName);
    }

    [Fact]
    public async Task AuditLoggingPersistence_ShouldReturnAppendedRecords()
    {
        var services = CreateServices();

        await using var serviceProvider = services.BuildServiceProvider();
        var auditLogStore = serviceProvider.GetRequiredService<IAuditLogStore>();

        await auditLogStore.AppendAsync(
            new AuditLogRecord(
                "workspace.audit.seed",
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                TimeSpan.Zero,
                null,
                null,
                null,
                false,
                null,
                true,
                null,
                new Dictionary<string, object?>()),
            TestContext.Current.CancellationToken);

        var records = await auditLogStore.GetListAsync(TestContext.Current.CancellationToken);
        records.Count.ShouldBe(1);
        records.Single().Name.ShouldBe("workspace.audit.seed");
    }

    private static ServiceCollection CreateServices()
    {
        var databaseName = $"audit-logging-{Guid.NewGuid():N}";
        var services = new ServiceCollection();
        services.AddModule<AuditLoggingPersistenceTestModule>();
        services.UseDbContextOptions(options =>
            options.UseInMemoryDatabase(databaseName));

        return services;
    }
}
