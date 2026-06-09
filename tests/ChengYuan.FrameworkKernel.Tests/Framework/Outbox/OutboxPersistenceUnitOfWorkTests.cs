using ChengYuan.Core.Data;
using ChengYuan.Core.Modularity;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class OutboxPersistenceUnitOfWorkTests
{
    [Fact]
    public async Task Outbox_ShouldNotPersistBeforeUnitOfWorkCompletes()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"outbox-uow-{Guid.NewGuid():N}.db");
        var services = new ServiceCollection();
        services.UseDbContextOptions(options =>
            options.UseSqlite($"Data Source={databasePath}"));
        services.AddModule<OutboxPersistenceUnitOfWorkTestModule>();

        await using var serviceProvider = services.BuildServiceProvider();
        await using (var setupScope = serviceProvider.CreateAsyncScope())
        {
            await setupScope.ServiceProvider.GetRequiredService<OutboxDbContext>()
                .Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        }

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var outbox = scope.ServiceProvider.GetRequiredService<IOutbox>();
            var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await outbox.EnqueueAsync(
                "workspace.created",
                new WorkspaceCreated("workspace-a"),
                TestContext.Current.CancellationToken);

            var pendingMessages = await store.GetPendingAsync(10, TestContext.Current.CancellationToken);
            pendingMessages.ShouldBeEmpty();

            await using (var beforeCompleteScope = serviceProvider.CreateAsyncScope())
            {
                var dbContext = beforeCompleteScope.ServiceProvider.GetRequiredService<OutboxDbContext>();
                var messages = await dbContext.OutboxMessages.ToArrayAsync(TestContext.Current.CancellationToken);
                messages.ShouldBeEmpty();
            }

            await unitOfWork.CompleteAsync(TestContext.Current.CancellationToken);
        }

        await using (var verificationScope = serviceProvider.CreateAsyncScope())
        {
            var dbContext = verificationScope.ServiceProvider.GetRequiredService<OutboxDbContext>();
            var messages = await dbContext.OutboxMessages.ToArrayAsync(TestContext.Current.CancellationToken);
            messages.Length.ShouldBe(1);
            messages[0].Name.ShouldBe("workspace.created");
        }
    }

    [Fact]
    public async Task Outbox_ShouldReturnPendingMessagesInCreatedOrderWithDatabaseLimit()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"outbox-pending-order-{Guid.NewGuid():N}.db");
        var services = new ServiceCollection();
        services.UseDbContextOptions(options =>
            options.UseSqlite($"Data Source={databasePath}"));
        services.AddModule<OutboxPersistenceUnitOfWorkTestModule>();

        await using var serviceProvider = services.BuildServiceProvider();
        await using (var setupScope = serviceProvider.CreateAsyncScope())
        {
            await setupScope.ServiceProvider.GetRequiredService<OutboxDbContext>()
                .Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        }

        await using (var writeScope = serviceProvider.CreateAsyncScope())
        {
            var store = writeScope.ServiceProvider.GetRequiredService<IOutboxStore>();
            var unitOfWork = writeScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await store.SaveAsync(CreateMessage("third", new DateTimeOffset(2026, 1, 3, 0, 0, 0, TimeSpan.Zero)), TestContext.Current.CancellationToken);
            await store.SaveAsync(CreateMessage("first", new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)), TestContext.Current.CancellationToken);
            await store.SaveAsync(CreateMessage("second", new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero)), TestContext.Current.CancellationToken);
            await unitOfWork.CompleteAsync(TestContext.Current.CancellationToken);
        }

        await using (var readScope = serviceProvider.CreateAsyncScope())
        {
            var store = readScope.ServiceProvider.GetRequiredService<IOutboxStore>();

            var messages = await store.GetPendingAsync(2, TestContext.Current.CancellationToken);

            messages.Select(message => message.Name).ShouldBe(["first", "second"]);
        }
    }

    [Fact]
    public async Task OutboxWorker_ShouldDispatchMultipleEfMessagesInOneDrain()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"outbox-worker-batch-{Guid.NewGuid():N}.db");
        var services = new ServiceCollection();
        services.UseDbContextOptions(options =>
            options.UseSqlite($"Data Source={databasePath}"));
        services.AddSingleton<RecordingOutboxDispatcher>();
        services.AddSingleton<IOutboxDispatcher>(serviceProvider => serviceProvider.GetRequiredService<RecordingOutboxDispatcher>());
        services.AddModule<OutboxPersistenceWorkerTestModule>();

        await using var serviceProvider = services.BuildServiceProvider();
        await using (var setupScope = serviceProvider.CreateAsyncScope())
        {
            await setupScope.ServiceProvider.GetRequiredService<OutboxDbContext>()
                .Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        }

        await using (var writeScope = serviceProvider.CreateAsyncScope())
        {
            var outbox = writeScope.ServiceProvider.GetRequiredService<IOutbox>();
            var unitOfWork = writeScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await outbox.EnqueueAsync("workspace.created", new WorkspaceCreated("workspace-a"), TestContext.Current.CancellationToken);
            await outbox.EnqueueAsync("workspace.updated", new WorkspaceCreated("workspace-b"), TestContext.Current.CancellationToken);
            await unitOfWork.CompleteAsync(TestContext.Current.CancellationToken);
        }

        await using (var workerScope = serviceProvider.CreateAsyncScope())
        {
            var worker = workerScope.ServiceProvider.GetRequiredService<IOutboxWorker>();
            var result = await worker.DrainAsync(10, TestContext.Current.CancellationToken);

            result.AttemptedCount.ShouldBe(2);
            result.DispatchedCount.ShouldBe(2);
            result.FailedCount.ShouldBe(0);
        }

        var dispatcher = serviceProvider.GetRequiredService<RecordingOutboxDispatcher>();
        dispatcher.MessageNames.ShouldBe(["workspace.created", "workspace.updated"]);

        await using (var verificationScope = serviceProvider.CreateAsyncScope())
        {
            var dbContext = verificationScope.ServiceProvider.GetRequiredService<OutboxDbContext>();
            var messages = await dbContext.OutboxMessages
                .OrderBy(message => message.CreatedAtUtcTicks)
                .ThenBy(message => message.Id)
                .ToArrayAsync(TestContext.Current.CancellationToken);

            messages.Length.ShouldBe(2);
            messages.ShouldAllBe(message => message.Status == OutboxMessageStatus.Dispatched);
            messages.ShouldAllBe(message => message.AttemptCount == 1);
        }
    }

    [DependsOn(typeof(OutboxPersistenceModule))]
    private sealed class OutboxPersistenceUnitOfWorkTestModule : ExtensionModule;

    [DependsOn(typeof(OutboxWorkerModule))]
    [DependsOn(typeof(OutboxPersistenceModule))]
    private sealed class OutboxPersistenceWorkerTestModule : ExtensionModule
    {
        protected override IModuleDescriptor ResolveAttachedCapability(IModuleLoadContext context)
        {
            return Dependencies.Single(dependency => dependency.ModuleType == typeof(OutboxWorkerModule));
        }
    }

    private sealed record WorkspaceCreated(string Name);

    private sealed class RecordingOutboxDispatcher : IOutboxDispatcher
    {
        public List<string> MessageNames { get; } = [];

        public ValueTask DispatchAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            MessageNames.Add(message.Name);
            return ValueTask.CompletedTask;
        }
    }

    private static OutboxMessage CreateMessage(string name, DateTimeOffset createdAtUtc)
    {
        return new OutboxMessage(
            Guid.NewGuid(),
            name,
            new OutboxPayload([], typeof(WorkspaceCreated).AssemblyQualifiedName!),
            createdAtUtc,
            TenantId: null,
            CorrelationId: null,
            OutboxMessageStatus.Pending,
            AttemptCount: 0,
            DispatchedAtUtc: null,
            LastError: null);
    }
}
