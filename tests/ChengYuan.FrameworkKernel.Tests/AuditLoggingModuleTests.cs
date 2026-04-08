using ChengYuan.Auditing;
using ChengYuan.AuditLogging;
using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class AuditLoggingModuleTests
{
    [Fact]
    public void AuditLoggingModule_ShouldLoadIntoTheServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddModule<AuditLoggingTestModule>();
        services.AddInMemoryAuditLogging();

        using var serviceProvider = services.BuildServiceProvider();

        var moduleCatalog = serviceProvider.GetRequiredService<ModuleCatalog>();
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(AuditLoggingModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(AuditingModule));

        serviceProvider.GetRequiredService<IAuditLogStore>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IAuditLogReader>().ShouldNotBeNull();
    }

    [Fact]
    public async Task AuditLoggingModule_ShouldPersistAuditEntriesProducedByAuditing()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tenantId = Guid.NewGuid();
        var services = new ServiceCollection();
        services.AddModule<AuditLoggingTestModule>();
        services.AddInMemoryAuditLogging();

        using var serviceProvider = services.BuildServiceProvider();
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
            }, cancellationToken);
        }

        var records = await auditLogReader.GetListAsync(cancellationToken);
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
    public async Task AuditLoggingModule_ShouldPersistPropertiesAndFailures()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<AuditLoggingTestModule>();
        services.AddInMemoryAuditLogging();

        using var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IAuditScopeFactory>();
        var auditLogReader = serviceProvider.GetRequiredService<IAuditLogReader>();

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await scopeFactory.ExecuteAsync("workspace.audit.delete", _ => ValueTask.FromException(new InvalidOperationException("boom")), cancellationToken));

        var records = await auditLogReader.GetListAsync(cancellationToken);
        var record = records.Single();
        record.Succeeded.ShouldBeFalse();
        record.ErrorMessage.ShouldBe("boom");
        record.TryGetProperty("exceptionType", out var exceptionType).ShouldBeTrue();
        exceptionType.ShouldBe(typeof(InvalidOperationException).FullName);
    }

    [Fact]
    public async Task AuditLoggingStore_ShouldReturnAppendedRecords()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<AuditLoggingTestModule>();
        services.AddInMemoryAuditLogging();

        using var serviceProvider = services.BuildServiceProvider();
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
            cancellationToken);

        var records = await auditLogStore.GetListAsync(cancellationToken);
        records.Count.ShouldBe(1);
        records.Single().Name.ShouldBe("workspace.audit.seed");
    }

    [DependsOn(typeof(AuditLoggingModule))]
    private sealed class AuditLoggingTestModule : ModuleBase
    {
    }
}
