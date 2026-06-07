using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChengYuan.AuditLogging;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.Identity;
using ChengYuan.TenantManagement;
using ChengYuan.WebHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class AuditLoggingTenantScopeWebEndpointTests
{
    [Fact]
    public async Task AuditLogs_ShouldOnlyListCurrentTenantEntriesFromTenantContext()
    {
        await using var app = await CreateApplicationAsync();
        var currentTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        await SeedTenantsAsync(app, currentTenantId, otherTenantId);
        await SeedAuditLogAsync(app, "audit.current", currentTenantId);
        await SeedAuditLogAsync(app, "audit.other", otherTenantId);

        var client = await CreateAuthenticatedTenantClientAsync(app, currentTenantId);

        var response = await client.GetAsync("/api/v1/audit-logs", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<ItemsWrapper<AuditLogDto>>(TestContext.Current.CancellationToken);
        list.ShouldNotBeNull();
        list.Items.ShouldContain(log => log.Name == "audit.current" && log.TenantId == currentTenantId);
        list.Items.ShouldNotContain(log => log.Name == "audit.other" && log.TenantId == otherTenantId);
    }

    [Fact]
    public async Task AuditLogs_ShouldForbidTenantTokenWithAnotherTenantHeader()
    {
        await using var app = await CreateApplicationAsync();
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        await SeedTenantsAsync(app, tenantId, otherTenantId);

        var client = await CreateAuthenticatedTenantClientAsync(app, tenantId, otherTenantId);

        var response = await client.GetAsync("/api/v1/audit-logs", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private static async Task<WebApplication> CreateApplicationAsync()
    {
        var databaseName = $"audit-scope-web-{Guid.NewGuid():N}";
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.UseDbContextOptions(options => options.UseInMemoryDatabase(databaseName));
        builder.AddTestWebHost();

        var app = builder.Build();
        app.UseWebHostComposition();
        await app.StartAsync();

        return app;
    }

    private static async Task<HttpClient> CreateAuthenticatedTenantClientAsync(WebApplication app, Guid tenantId, Guid? headerTenantId = null)
    {
        var userManager = app.Services.GetRequiredService<IUserManager>();
        var membershipManager = app.Services.GetRequiredService<IUserTenantMembershipManager>();
        var user = await userManager.CreateAsync(
            $"tenant-{Guid.NewGuid():N}",
            $"tenant-{Guid.NewGuid():N}@test.com",
            "TenantPass123!",
            TestContext.Current.CancellationToken);
        await membershipManager.AssignAsync(user.Id, tenantId, TestContext.Current.CancellationToken);

        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestJwtTokenFactory.CreateAccessToken(tenantId, user.Id));
        client.DefaultRequestHeaders.Add("X-Tenant-Id", (headerTenantId ?? tenantId).ToString());

        return client;
    }

    private static async Task SeedTenantsAsync(WebApplication app, params Guid[] tenantIds)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>();

        foreach (var tenantId in tenantIds)
        {
            await dbContext.Tenants.AddAsync(
                new TenantEntity(tenantId, $"Tenant-{tenantId:N}", isActive: true),
                TestContext.Current.CancellationToken);
        }

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static async Task SeedAuditLogAsync(WebApplication app, string name, Guid tenantId)
    {
        using var scope = app.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IAuditLogStore>();

        await store.AppendAsync(
            new AuditLogRecord(
                name,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                TimeSpan.Zero,
                tenantId,
                userId: null,
                userName: null,
                isAuthenticated: false,
                correlationId: null,
                succeeded: true,
                errorMessage: null,
                properties: new Dictionary<string, object?>(),
                entityChanges: []),
            TestContext.Current.CancellationToken);
    }

    private sealed class AuditLogDto
    {
        public string Name { get; set; } = string.Empty;

        public Guid? TenantId { get; set; }
    }

    private sealed class ItemsWrapper<T>
    {
        public List<T> Items { get; set; } = [];
    }
}
