using ChengYuan.Auditing;
using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class AuditingModuleTests
{
    [Fact]
    public void AuditingModule_ShouldLoadIntoTheServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddModule<AuditingTestModule>();

        using var serviceProvider = services.BuildServiceProvider();

        var moduleCatalog = serviceProvider.GetRequiredService<ModuleCatalog>();
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(AuditingModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(ExecutionContextModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(MultiTenancyModule));

        serviceProvider.GetRequiredService<IAuditScopeFactory>().ShouldNotBeNull();
    }

    [Fact]
    public async Task AuditScopeFactory_ShouldCaptureAmbientContext()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<AuditingTestModule>();
        services.AddInMemoryAuditing();

        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IAuditScopeFactory>();
        var collector = serviceProvider.GetRequiredService<InMemoryAuditLogCollector>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();
        var currentCorrelation = serviceProvider.GetRequiredService<ICurrentCorrelationAccessor>();
        var tenantId = Guid.NewGuid();

        using (currentTenant.Change(tenantId, "tenant-a"))
        using (currentUser.Change(new CurrentUserInfo("alice", "Alice", true)))
        using (currentCorrelation.Change("corr-123"))
        {
            await factory.ExecuteAsync("workspace.members.invite", _ => ValueTask.CompletedTask, cancellationToken);
        }

        collector.Entries.Count.ShouldBe(1);
        var entry = collector.Entries.Single();
        entry.Name.ShouldBe("workspace.members.invite");
        entry.TenantId.ShouldBe(tenantId);
        entry.UserId.ShouldBe("alice");
        entry.UserName.ShouldBe("Alice");
        entry.IsAuthenticated.ShouldBeTrue();
        entry.CorrelationId.ShouldBe("corr-123");
        entry.Succeeded.ShouldBeTrue();
        entry.CompletedAtUtc.ShouldNotBeNull();
        entry.Duration.ShouldNotBeNull();
    }

    [Fact]
    public async Task AuditScopeFactory_ShouldRecordFailureDetails()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<AuditingTestModule>();
        services.AddInMemoryAuditing();

        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IAuditScopeFactory>();
        var collector = serviceProvider.GetRequiredService<InMemoryAuditLogCollector>();

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await factory.ExecuteAsync(
                "workspace.members.delete",
                _ => ValueTask.FromException(new InvalidOperationException("delete failed")),
                cancellationToken));

        collector.Entries.Count.ShouldBe(1);
        var entry = collector.Entries.Single();
        entry.Succeeded.ShouldBeFalse();
        entry.ErrorMessage.ShouldBe("delete failed");
        entry.TryGetProperty("exceptionType", out var exceptionType).ShouldBeTrue();
        exceptionType.ShouldBe(typeof(InvalidOperationException).FullName);
    }

    [Fact]
    public async Task AuditScope_ShouldAllowCustomProperties()
    {
        var services = new ServiceCollection();
        services.AddModule<AuditingTestModule>();
        services.AddInMemoryAuditing();

        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IAuditScopeFactory>();
        var collector = serviceProvider.GetRequiredService<InMemoryAuditLogCollector>();

        await using (var scope = factory.Create("workspace.members.update"))
        {
            scope.SetProperty("memberCount", 12);
            scope.MarkSucceeded();
        }

        var entry = collector.Entries.Single();
        entry.TryGetProperty("memberCount", out var memberCount).ShouldBeTrue();
        memberCount.ShouldBe(12);
    }

    [Fact]
    public async Task AuditContributors_ShouldEnrichEntriesBeforeWriting()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddSingleton<IAuditLogContributor, TestAuditContributor>();
        services.AddModule<AuditingTestModule>();
        services.AddInMemoryAuditing();

        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IAuditScopeFactory>();
        var collector = serviceProvider.GetRequiredService<InMemoryAuditLogCollector>();

        await factory.ExecuteAsync("workspace.members.list", _ => ValueTask.CompletedTask, cancellationToken);

        var entry = collector.Entries.Single();
        entry.TryGetProperty("contributor", out var contributor).ShouldBeTrue();
        contributor.ShouldBe("test");
    }

    [DependsOn(typeof(AuditingModule))]
    private sealed class AuditingTestModule : ModuleBase
    {
    }

    private sealed class TestAuditContributor : IAuditLogContributor
    {
        public ValueTask ContributeAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
        {
            entry.SetProperty("contributor", "test");
            return ValueTask.CompletedTask;
        }
    }
}
