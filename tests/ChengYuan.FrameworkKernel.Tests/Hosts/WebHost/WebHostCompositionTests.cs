using System.Collections.Concurrent;
using System.Net;
using ChengYuan.AuditLogging;
using ChengYuan.Core.Data;
using ChengYuan.Core.Entities;
using ChengYuan.Core.Modularity;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.EventBus;
using ChengYuan.FeatureManagement;
using ChengYuan.Hosting;
using ChengYuan.Identity;
using ChengYuan.MultiTenancy;
using ChengYuan.Outbox;
using ChengYuan.PermissionManagement;
using ChengYuan.SettingManagement;
using ChengYuan.Settings;
using ChengYuan.TenantManagement;
using ChengYuan.WebHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class WebHostCompositionTests
{
    [Fact]
    public void WebHostSqliteComposition_ShouldRegisterSqliteProviderModuleAndUseSqliteDbContexts()
    {
        var connectionString = $"Data Source={Path.Combine(Path.GetTempPath(), $"chengyuan-web-{Guid.NewGuid():N}.db")}";
        var services = new ServiceCollection();
        services.UseSqlite(connectionString);
        services.AddTestWebHost();

        using var serviceProvider = services.BuildServiceProvider();
        var moduleCatalog = serviceProvider.GetRequiredService<ChengYuan.Core.Modularity.ModuleCatalog>();
        var moduleNames = moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ToArray();

        moduleNames.ShouldContain(nameof(WebHostHttpCompositionModule));
        moduleNames.ShouldContain(nameof(WebHostFrameworkCompositionModule));
        moduleNames.ShouldContain(nameof(WebHostApplicationCompositionModule));
        moduleNames.ShouldContain(nameof(WebHostRuntimeGlueModule));
        moduleNames.ShouldContain("ChengYuanEntityFrameworkCoreSqliteModule");

        using var scope = serviceProvider.CreateScope();
        var identityProviderName = scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.ProviderName;
        var tenantProviderName = scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>().Database.ProviderName;
        var outboxProviderName = scope.ServiceProvider.GetRequiredService<OutboxDbContext>().Database.ProviderName;

        identityProviderName.ShouldNotBeNull();
        tenantProviderName.ShouldNotBeNull();
        outboxProviderName.ShouldNotBeNull();
        identityProviderName.ShouldContain("Sqlite");
        tenantProviderName.ShouldContain("Sqlite");
        outboxProviderName.ShouldContain("Sqlite");
    }

    [Fact]
    public void WebHostComposition_ShouldRegisterIdentityWebAndPersistenceBackedModules()
    {
        var databaseName = $"composition-{Guid.NewGuid():N}";
        var services = new ServiceCollection();
        services.UseDbContextOptions(options => options.UseInMemoryDatabase(databaseName));
        services.AddTestWebHost();

        using var serviceProvider = services.BuildServiceProvider();
        var moduleCatalog = serviceProvider.GetRequiredService<ChengYuan.Core.Modularity.ModuleCatalog>();
        var moduleNames = moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ToArray();

        moduleNames.ShouldContain(nameof(WebHostHttpCompositionModule));
        moduleNames.ShouldContain(nameof(WebHostFrameworkCompositionModule));
        moduleNames.ShouldContain(nameof(WebHostApplicationCompositionModule));
        moduleNames.ShouldContain(nameof(WebHostRuntimeGlueModule));
        moduleNames.ShouldContain(nameof(IdentityWebModule));
        moduleNames.ShouldContain(nameof(IdentityPersistenceModule));
        moduleNames.ShouldContain(nameof(TenantManagementPersistenceModule));
        moduleNames.ShouldContain(nameof(SettingManagementPersistenceModule));
        moduleNames.ShouldContain(nameof(PermissionManagementPersistenceModule));
        moduleNames.ShouldContain(nameof(FeatureManagementPersistenceModule));
        moduleNames.ShouldContain(nameof(AuditLoggingPersistenceModule));
        moduleNames.ShouldContain(nameof(OutboxPersistenceModule));
        moduleNames.ShouldContain("EventBusModule");
        moduleNames.ShouldContain("EventBusOutboxModule");
        moduleNames.ShouldContain(nameof(OutboxWorkerModule));

        using var scope = serviceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IdentityDbContext>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<SettingManagementDbContext>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<PermissionManagementDbContext>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<FeatureManagementDbContext>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<AuditLoggingDbContext>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<OutboxDbContext>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<IOutboxWorker>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<IUserManager>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<IRoleManager>().ShouldNotBeNull();
    }

    [Fact]
    public async Task WebHostMessaging_ShouldPersistDistributedEventsToOutboxAndDispatchToLocalSubscribers()
    {
        var databaseName = $"chengyuan-messaging-{Guid.NewGuid():N}";
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.UseDbContextOptions(options => options.UseInMemoryDatabase(databaseName));
        RecordingMessageSubscriber.Clear();
        builder.Services.AddEventSubscriber<WorkspaceProvisioned, RecordingMessageSubscriber>();
        builder.AddTestWebHost();

        await using var app = builder.Build();
        app.UseWebHostComposition();
        app.MapPost("/messaging/provision-workspace", static async (
            IDistributedEventBus eventBus,
            CancellationToken cancellationToken) =>
        {
            await eventBus.PublishAsync(new WorkspaceProvisioned("workspace-a"), cancellationToken);
            return Results.Accepted();
        });

        await app.StartAsync(TestContext.Current.CancellationToken);

        var response = await app.GetTestClient().PostAsync("/messaging/provision-workspace", null, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);

        await using (var verificationScope = app.Services.CreateAsyncScope())
        {
            var outboxDbContext = verificationScope.ServiceProvider.GetRequiredService<OutboxDbContext>();
            var pendingMessages = await outboxDbContext.OutboxMessages
                .Where(message => message.Status == OutboxMessageStatus.Pending)
                .ToArrayAsync(TestContext.Current.CancellationToken);

            pendingMessages.Length.ShouldBe(1);
            pendingMessages[0].Name.ShouldBe(typeof(WorkspaceProvisioned).FullName);
        }

        await using (var workerScope = app.Services.CreateAsyncScope())
        {
            var worker = workerScope.ServiceProvider.GetRequiredService<IOutboxWorker>();
            var result = await worker.DrainAsync(10, TestContext.Current.CancellationToken);

            result.AttemptedCount.ShouldBe(1);
            result.DispatchedCount.ShouldBe(1);
            result.FailedCount.ShouldBe(0);
        }

        RecordingMessageSubscriber.WorkspaceNames.ShouldBe(["workspace-a"]);

        await using (var dispatchedScope = app.Services.CreateAsyncScope())
        {
            var outboxDbContext = dispatchedScope.ServiceProvider.GetRequiredService<OutboxDbContext>();
            var message = await outboxDbContext.OutboxMessages.SingleAsync(TestContext.Current.CancellationToken);
            message.Status.ShouldBe(OutboxMessageStatus.Dispatched);
            message.AttemptCount.ShouldBe(1);
            message.DispatchedAtUtc.ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task WebHostMessaging_ShouldPersistDomainEventsToOutboxBeforeDispatching()
    {
        var databaseName = $"chengyuan-domain-messaging-{Guid.NewGuid():N}";
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.UseDbContextOptions(options => options.UseInMemoryDatabase(databaseName));
        builder.Services.AddConfiguredDbContext<MessagingTestDbContext>();
        builder.Services.AddEntityFrameworkCoreDataAccess<MessagingTestDbContext>();
        DomainMessageSubscriber.Clear();
        builder.Services.AddEventSubscriber<WorkspaceDomainProvisioned, DomainMessageSubscriber>();
        builder.AddTestWebHost();

        await using var app = builder.Build();
        app.UseWebHostComposition();
        app.MapPost("/messaging/domain-workspace", static async (
            MessagingTestDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            await dbContext.Workspaces.AddAsync(WorkspaceAggregate.Create("workspace-domain"), cancellationToken);
            return Results.Accepted();
        });

        await app.StartAsync(TestContext.Current.CancellationToken);

        var response = await app.GetTestClient().PostAsync("/messaging/domain-workspace", null, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        DomainMessageSubscriber.WorkspaceNames.ShouldBeEmpty();

        await using (var verificationScope = app.Services.CreateAsyncScope())
        {
            var outboxDbContext = verificationScope.ServiceProvider.GetRequiredService<OutboxDbContext>();
            var pendingMessages = await outboxDbContext.OutboxMessages
                .Where(message => message.Status == OutboxMessageStatus.Pending)
                .ToArrayAsync(TestContext.Current.CancellationToken);

            pendingMessages.Length.ShouldBe(1);
            pendingMessages[0].Name.ShouldBe(typeof(WorkspaceDomainProvisioned).FullName);
        }

        await using (var workerScope = app.Services.CreateAsyncScope())
        {
            var worker = workerScope.ServiceProvider.GetRequiredService<IOutboxWorker>();
            var result = await worker.DrainAsync(10, TestContext.Current.CancellationToken);

            result.AttemptedCount.ShouldBe(1);
            result.DispatchedCount.ShouldBe(1);
            result.FailedCount.ShouldBe(0);
        }

        DomainMessageSubscriber.WorkspaceNames.ShouldBe(["workspace-domain"]);
    }

    [Fact]
    public async Task WebHostMessaging_ShouldMarkOutboxMessageFailed_WhenSubscriberThrows()
    {
        var databaseName = $"chengyuan-failing-messaging-{Guid.NewGuid():N}";
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.UseDbContextOptions(options => options.UseInMemoryDatabase(databaseName));
        builder.Services.AddEventSubscriber<WorkspaceProvisioningFailed, FailingMessageSubscriber>();
        builder.AddTestWebHost();

        await using var app = builder.Build();
        app.UseWebHostComposition();
        app.MapPost("/messaging/failing-workspace", static async (
            IDistributedEventBus eventBus,
            CancellationToken cancellationToken) =>
        {
            await eventBus.PublishAsync(new WorkspaceProvisioningFailed("workspace-failing"), cancellationToken);
            return Results.Accepted();
        });

        await app.StartAsync(TestContext.Current.CancellationToken);

        var response = await app.GetTestClient().PostAsync("/messaging/failing-workspace", null, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);

        await using (var workerScope = app.Services.CreateAsyncScope())
        {
            var worker = workerScope.ServiceProvider.GetRequiredService<IOutboxWorker>();
            var result = await worker.DrainAsync(10, TestContext.Current.CancellationToken);

            result.AttemptedCount.ShouldBe(1);
            result.DispatchedCount.ShouldBe(0);
            result.FailedCount.ShouldBe(1);
        }

        await using (var verificationScope = app.Services.CreateAsyncScope())
        {
            var outboxDbContext = verificationScope.ServiceProvider.GetRequiredService<OutboxDbContext>();
            var message = await outboxDbContext.OutboxMessages.SingleAsync(TestContext.Current.CancellationToken);

            message.Name.ShouldBe(typeof(WorkspaceProvisioningFailed).FullName);
            message.Status.ShouldBe(OutboxMessageStatus.Failed);
            message.AttemptCount.ShouldBe(1);
            message.LastError.ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task WebHostComposition_ShouldPassServiceProviderValidation()
    {
        var databaseName = $"composition-validation-{Guid.NewGuid():N}";
        var builder = WebApplication.CreateBuilder();
        builder.Services.UseDbContextOptions(options => options.UseInMemoryDatabase(databaseName));
        builder.AddTestWebHost();

        await using var app = builder.Build();

        app.Services.ShouldNotBeNull();
    }

    [Fact]
    public void WebModules_ShouldLoadWithoutPersistenceModules()
    {
        var services = new ServiceCollection();
        services.AddChengYuan<WebOnlyHostModule>(cy => cy
            .AddModule<IdentityWebModule>()
            .AddModule<TenantManagementWebModule>()
            .AddModule<SettingManagementWebModule>()
            .AddModule<PermissionManagementWebModule>()
            .AddModule<FeatureManagementWebModule>()
            .AddModule<AuditLoggingWebModule>());

        using var serviceProvider = services.BuildServiceProvider();
        var moduleCatalog = serviceProvider.GetRequiredService<ChengYuan.Core.Modularity.ModuleCatalog>();
        var moduleNames = moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ToArray();

        moduleNames.ShouldContain(nameof(IdentityWebModule));
        moduleNames.ShouldContain(nameof(TenantManagementWebModule));
        moduleNames.ShouldContain(nameof(SettingManagementWebModule));
        moduleNames.ShouldContain(nameof(PermissionManagementWebModule));
        moduleNames.ShouldContain(nameof(FeatureManagementWebModule));
        moduleNames.ShouldContain(nameof(AuditLoggingWebModule));
        moduleNames.ShouldNotContain(nameof(IdentityPersistenceModule));
        moduleNames.ShouldNotContain(nameof(TenantManagementPersistenceModule));
        moduleNames.ShouldNotContain(nameof(SettingManagementPersistenceModule));
        moduleNames.ShouldNotContain(nameof(PermissionManagementPersistenceModule));
        moduleNames.ShouldNotContain(nameof(FeatureManagementPersistenceModule));
        moduleNames.ShouldNotContain(nameof(AuditLoggingPersistenceModule));
        moduleNames.ShouldNotContain(nameof(OutboxPersistenceModule));
    }

    [Fact]
    public async Task WebHostComposition_ShouldCommitChangesAcrossModuleDbContexts()
    {
        var databaseName = $"composition-uow-{Guid.NewGuid():N}";
        var services = new ServiceCollection();
        services.UseDbContextOptions(options => options.UseInMemoryDatabase(databaseName));
        services.AddTestWebHost();

        await using var serviceProvider = services.BuildServiceProvider();

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var identityDbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            var tenantDbContext = scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await identityDbContext.Roles.AddAsync(new IdentityRole(Guid.NewGuid(), "Administrators"), TestContext.Current.CancellationToken);
            await tenantDbContext.Tenants.AddAsync(new TenantEntity(Guid.NewGuid(), "Default", isActive: true), TestContext.Current.CancellationToken);

            await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var verificationScope = serviceProvider.CreateAsyncScope())
        {
            var identityDbContext = verificationScope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            var tenantDbContext = verificationScope.ServiceProvider.GetRequiredService<TenantManagementDbContext>();

            var roleNames = await identityDbContext.Roles.Select(role => role.Name).OrderBy(name => name).ToArrayAsync(TestContext.Current.CancellationToken);
            var tenantNames = await tenantDbContext.Tenants.Select(tenant => tenant.Name).OrderBy(name => name).ToArrayAsync(TestContext.Current.CancellationToken);

            roleNames.ShouldBe(["Administrators"]);
            tenantNames.ShouldBe(["Default"]);
        }
    }

    [Fact]
    public async Task WebHostComposition_ShouldRollbackCommandRequestChanges_WhenEndpointThrows()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"chengyuan-uow-rollback-{Guid.NewGuid():N}.db");
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.UseSqlite($"Data Source={databasePath}");
        builder.AddTestWebHost();

        await using var app = builder.Build();
        app.UseWebHostComposition();
        app.MapPost("/uow/failing-command", static async (
            ITenantManager tenantManager,
            ISettingValueManager settingValueManager,
            CancellationToken cancellationToken) =>
        {
            await tenantManager.CreateAsync("Acme", cancellationToken: cancellationToken);
            await settingValueManager.SetAsync(
                new SettingValueRecord("workspace.title", SettingScope.Global, "Main"),
                cancellationToken);

            throw new InvalidOperationException("Command failed after data changes.");
        });

        await EnsureDatabasesCreatedAsync(app.Services);
        await app.StartAsync(TestContext.Current.CancellationToken);

        var response = await app.GetTestClient().PostAsync("/uow/failing-command", null, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);

        await using var scope = app.Services.CreateAsyncScope();
        var tenantDbContext = scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>();
        var settingDbContext = scope.ServiceProvider.GetRequiredService<SettingManagementDbContext>();

        (await tenantDbContext.Tenants.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
        (await settingDbContext.SettingValues.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task WebHostComposition_ShouldExposeTenantResolutionStoreFromPersistence()
    {
        var databaseName = $"composition-resolution-store-{Guid.NewGuid():N}";
        var services = new ServiceCollection();
        services.UseDbContextOptions(options => options.UseInMemoryDatabase(databaseName));
        services.AddTestWebHost();

        await using var serviceProvider = services.BuildServiceProvider();
        var tenantId = Guid.NewGuid();

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var tenantDbContext = scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>();
            await tenantDbContext.Tenants.AddAsync(new TenantEntity(tenantId, "Acme", isActive: true), TestContext.Current.CancellationToken);
            await tenantDbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var verificationScope = serviceProvider.CreateAsyncScope())
        {
            var resolutionStore = verificationScope.ServiceProvider.GetRequiredService<ITenantResolutionStore>();

            var byId = await resolutionStore.FindByIdAsync(tenantId, TestContext.Current.CancellationToken);
            var byName = await resolutionStore.FindByNameAsync("acme", TestContext.Current.CancellationToken);

            byId.ShouldNotBeNull();
            byId.Id.ShouldBe(tenantId);
            byId.Name.ShouldBe("Acme");
            byId.IsActive.ShouldBeTrue();
            byName.ShouldNotBeNull();
            byName.Id.ShouldBe(tenantId);
        }
    }

    [Fact]
    public async Task PermissionPolicy_ShouldRequireAuthenticatedUser()
    {
        var databaseName = $"composition-permission-auth-{Guid.NewGuid():N}";
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.UseDbContextOptions(options => options.UseInMemoryDatabase(databaseName));
        builder.AddTestWebHost();

        await using var app = builder.Build();
        app.UseWebHostComposition();
        app.MapGet("/permission-only", static () => Results.Ok())
            .RequireAuthorization(IdentityPermissions.Users);
        await app.StartAsync(TestContext.Current.CancellationToken);

        var response = await app.GetTestClient().GetAsync("/permission-only", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private sealed class WebOnlyHostModule : HostModule;

    private sealed record WorkspaceProvisioned(string WorkspaceName);

    private sealed record WorkspaceDomainProvisioned(string WorkspaceName) : IDomainEvent;

    private sealed record WorkspaceProvisioningFailed(string WorkspaceName);

    private sealed class WorkspaceAggregate : AggregateRoot<Guid>
    {
        private WorkspaceAggregate(Guid id, string name)
            : base(id)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public static WorkspaceAggregate Create(string name)
        {
            var workspace = new WorkspaceAggregate(Guid.NewGuid(), name);
            workspace.AddDomainEvent(new WorkspaceDomainProvisioned(name));
            return workspace;
        }
    }

    private sealed class MessagingTestDbContext(DbContextOptions<MessagingTestDbContext> options) : DbContext(options)
    {
        public DbSet<WorkspaceAggregate> Workspaces => Set<WorkspaceAggregate>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkspaceAggregate>(builder =>
            {
                builder.HasKey(workspace => workspace.Id);
                builder.Property(workspace => workspace.Name).IsRequired();
            });
        }
    }

    private sealed class RecordingMessageSubscriber : IEventSubscriber<WorkspaceProvisioned>
    {
        private static readonly ConcurrentQueue<string> RecordedWorkspaceNames = [];

        public static string[] WorkspaceNames => RecordedWorkspaceNames.ToArray();

        public static void Clear()
        {
            RecordedWorkspaceNames.Clear();
        }

        public ValueTask HandleAsync(WorkspaceProvisioned eventData, CancellationToken cancellationToken = default)
        {
            RecordedWorkspaceNames.Enqueue(eventData.WorkspaceName);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class DomainMessageSubscriber : IEventSubscriber<WorkspaceDomainProvisioned>
    {
        private static readonly ConcurrentQueue<string> RecordedWorkspaceNames = [];

        public static string[] WorkspaceNames => RecordedWorkspaceNames.ToArray();

        public static void Clear()
        {
            RecordedWorkspaceNames.Clear();
        }

        public ValueTask HandleAsync(WorkspaceDomainProvisioned eventData, CancellationToken cancellationToken = default)
        {
            RecordedWorkspaceNames.Enqueue(eventData.WorkspaceName);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FailingMessageSubscriber : IEventSubscriber<WorkspaceProvisioningFailed>
    {
        public ValueTask HandleAsync(WorkspaceProvisioningFailed eventData, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException($"Unable to provision workspace '{eventData.WorkspaceName}'.");
        }
    }

    private static async Task EnsureDatabasesCreatedAsync(IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.MigrateAsync(TestContext.Current.CancellationToken);
        await scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>().Database.MigrateAsync(TestContext.Current.CancellationToken);
        await scope.ServiceProvider.GetRequiredService<SettingManagementDbContext>().Database.MigrateAsync(TestContext.Current.CancellationToken);
        await scope.ServiceProvider.GetRequiredService<PermissionManagementDbContext>().Database.MigrateAsync(TestContext.Current.CancellationToken);
        await scope.ServiceProvider.GetRequiredService<FeatureManagementDbContext>().Database.MigrateAsync(TestContext.Current.CancellationToken);
        await scope.ServiceProvider.GetRequiredService<AuditLoggingDbContext>().Database.MigrateAsync(TestContext.Current.CancellationToken);
        await scope.ServiceProvider.GetRequiredService<OutboxDbContext>().Database.MigrateAsync(TestContext.Current.CancellationToken);
    }
}
