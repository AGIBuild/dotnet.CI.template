using ChengYuan.Caching;
using ChengYuan.Core.Extensions;
using ChengYuan.Core.Guids;
using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using ChengYuan.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class FrameworkKernelTests
{
    [Fact]
    public void KernelModules_ShouldLoadIntoTheServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddModule<KernelTestModule>();

        using var serviceProvider = services.BuildServiceProvider();

        var moduleCatalog = serviceProvider.GetRequiredService<ModuleCatalog>();
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(ExecutionContextModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(MultiTenancyModule));

        serviceProvider.GetRequiredService<ICurrentUser>().IsAuthenticated.ShouldBeFalse();
        serviceProvider.GetRequiredService<ICurrentCorrelation>().CorrelationId.ShouldNotBeNullOrWhiteSpace();
        serviceProvider.GetRequiredService<ICurrentTenant>().IsAvailable.ShouldBeFalse();
        serviceProvider.GetRequiredService<IGuidGenerator>().Create().ShouldNotBe(Guid.Empty);
        serviceProvider.GetRequiredService<ExtraPropertyManager>().ShouldNotBeNull();
    }

    [Fact]
    public void GuidGenerator_ShouldProduceDifferentGuids()
    {
        var services = new ServiceCollection();
        services.AddModule<KernelTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var guidGenerator = serviceProvider.GetRequiredService<IGuidGenerator>();

        var first = guidGenerator.Create();
        var second = guidGenerator.Create();

        first.ShouldNotBe(Guid.Empty);
        second.ShouldNotBe(Guid.Empty);
        first.ShouldNotBe(second);
    }

    [Fact]
    public void CurrentTenantAccessor_ShouldRestoreThePreviousTenant_WhenScopeEnds()
    {
        var services = new ServiceCollection();
        services.AddModule<KernelTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var currentTenantAccessor = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();

        var tenantId = Guid.NewGuid();

        using (currentTenantAccessor.Change(tenantId, "tenant-a"))
        {
            currentTenantAccessor.IsAvailable.ShouldBeTrue();
            currentTenantAccessor.Id.ShouldBe(tenantId);
            currentTenantAccessor.Name.ShouldBe("tenant-a");
        }

        currentTenantAccessor.IsAvailable.ShouldBeFalse();
        currentTenantAccessor.Id.ShouldBeNull();
        currentTenantAccessor.Name.ShouldBeNull();
    }

    [Fact]
    public async Task CachingModule_ShouldCacheGlobalValues()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<CachingTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var cache = serviceProvider.GetRequiredService<IChengYuanCache>();
        var key = new ChengYuanCacheKey("kernel:global");

        await cache.SetAsync(key, "cached-value", new ChengYuanCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        }, cancellationToken);

        (await cache.ExistsAsync(key, cancellationToken)).ShouldBeTrue();
        (await cache.GetAsync<string>(key, cancellationToken)).ShouldBe("cached-value");
    }

    [Fact]
    public async Task CachingModule_ShouldIsolateTenantScopedValues()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<CachingTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var cache = serviceProvider.GetRequiredService<IChengYuanCache>();
        var currentTenantAccessor = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var key = new ChengYuanCacheKey("settings:timezone", ChengYuanCacheScope.Tenant);
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using (currentTenantAccessor.Change(tenantA, "tenant-a"))
        {
            await cache.SetAsync(key, "UTC+8", cancellationToken: cancellationToken);
        }

        using (currentTenantAccessor.Change(tenantB, "tenant-b"))
        {
            (await cache.GetAsync<string>(key, cancellationToken)).ShouldBeNull();
            await cache.SetAsync(key, "UTC", cancellationToken: cancellationToken);
        }

        using (currentTenantAccessor.Change(tenantA, "tenant-a"))
        {
            (await cache.GetAsync<string>(key, cancellationToken)).ShouldBe("UTC+8");
        }

        using (currentTenantAccessor.Change(tenantB, "tenant-b"))
        {
            (await cache.GetAsync<string>(key, cancellationToken)).ShouldBe("UTC");
        }
    }

    [Fact]
    public async Task OutboxModule_ShouldCaptureTenantAndCorrelationMetadata()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddSingleton<IOutboxDispatcher, RecordingOutboxDispatcher>();
        services.AddModule<OutboxTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var outbox = serviceProvider.GetRequiredService<IOutbox>();
        var store = serviceProvider.GetRequiredService<IOutboxStore>();
        var currentTenantAccessor = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var currentCorrelationAccessor = serviceProvider.GetRequiredService<ICurrentCorrelationAccessor>();
        var tenantId = Guid.NewGuid();

        using (currentTenantAccessor.Change(tenantId, "tenant-a"))
        using (currentCorrelationAccessor.Change("corr-001"))
        {
            await outbox.EnqueueAsync("workspace.created", new WorkspaceCreated("workspace-a"), cancellationToken);
        }

        var pendingMessages = await store.GetPendingAsync(10, cancellationToken);
        pendingMessages.Count.ShouldBe(1);
        pendingMessages[0].Name.ShouldBe("workspace.created");
        pendingMessages[0].TenantId.ShouldBe(tenantId);
        pendingMessages[0].CorrelationId.ShouldBe("corr-001");
        pendingMessages[0].Status.ShouldBe(OutboxMessageStatus.Pending);
    }

    [Fact]
    public async Task OutboxWorker_ShouldDispatchPendingMessages()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddSingleton<RecordingOutboxDispatcher>();
        services.AddSingleton<IOutboxDispatcher>(serviceProvider => serviceProvider.GetRequiredService<RecordingOutboxDispatcher>());
        services.AddModule<OutboxTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var outbox = serviceProvider.GetRequiredService<IOutbox>();
        var store = serviceProvider.GetRequiredService<IOutboxStore>();
        var worker = serviceProvider.GetRequiredService<IOutboxWorker>();
        var dispatcher = serviceProvider.GetRequiredService<RecordingOutboxDispatcher>();

        var messageId = await outbox.EnqueueAsync("workspace.archived", new WorkspaceCreated("workspace-b"), cancellationToken);
        var result = await worker.DrainAsync(10, cancellationToken);
        var message = await store.GetAsync(messageId, cancellationToken);

        result.AttemptedCount.ShouldBe(1);
        result.DispatchedCount.ShouldBe(1);
        result.FailedCount.ShouldBe(0);
        dispatcher.MessageNames.ShouldContain("workspace.archived");
        message.ShouldNotBeNull();
        message.Status.ShouldBe(OutboxMessageStatus.Dispatched);
        message.DispatchedAtUtc.ShouldNotBeNull();
    }

    [Fact]
    public async Task OutboxWorker_ShouldMarkFailedMessages_WhenDispatchThrows()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddSingleton<IOutboxDispatcher, FailingOutboxDispatcher>();
        services.AddModule<OutboxTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var outbox = serviceProvider.GetRequiredService<IOutbox>();
        var store = serviceProvider.GetRequiredService<IOutboxStore>();
        var worker = serviceProvider.GetRequiredService<IOutboxWorker>();

        var messageId = await outbox.EnqueueAsync("workspace.deleted", new WorkspaceCreated("workspace-c"), cancellationToken);
        var result = await worker.DrainAsync(10, cancellationToken);
        var message = await store.GetAsync(messageId, cancellationToken);

        result.AttemptedCount.ShouldBe(1);
        result.DispatchedCount.ShouldBe(0);
        result.FailedCount.ShouldBe(1);
        message.ShouldNotBeNull();
        message.Status.ShouldBe(OutboxMessageStatus.Failed);
        message.AttemptCount.ShouldBe(1);
        message.LastError.ShouldNotBeNullOrWhiteSpace();
    }

    [DependsOn(typeof(MultiTenancyModule))]
    private sealed class KernelTestModule : ModuleBase
    {
    }

    [DependsOn(typeof(MemoryCachingModule))]
    private sealed class CachingTestModule : ModuleBase
    {
    }

    [DependsOn(typeof(OutboxWorkerModule))]
    private sealed class OutboxTestModule : ModuleBase
    {
    }

    private sealed record WorkspaceCreated(string WorkspaceName);

    private sealed class RecordingOutboxDispatcher : IOutboxDispatcher
    {
        public List<string> MessageNames { get; } = [];

        public ValueTask DispatchAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            MessageNames.Add(message.Name);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FailingOutboxDispatcher : IOutboxDispatcher
    {
        public ValueTask DispatchAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException($"Unable to dispatch '{message.Name}'.");
        }
    }
}
