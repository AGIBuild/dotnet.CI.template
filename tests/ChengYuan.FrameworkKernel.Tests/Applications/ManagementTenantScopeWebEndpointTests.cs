using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChengYuan.Authorization;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.FeatureManagement;
using ChengYuan.Features;
using ChengYuan.Identity;
using ChengYuan.PermissionManagement;
using ChengYuan.TenantManagement;
using ChengYuan.WebHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class ManagementTenantScopeWebEndpointTests
{
    [Fact]
    public async Task Features_ShouldForbidWritingAnotherTenantValueFromTenantContext()
    {
        await using var app = await CreateApplicationAsync();
        var currentTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        await SeedTenantsAsync(app, currentTenantId, otherTenantId);

        var client = await CreateAuthenticatedTenantClientAsync(app, currentTenantId);

        var putResponse = await client.PutAsJsonAsync("/api/v1/features", new
        {
            name = "workspace.cross-tenant-feature",
            scope = FeatureScope.Tenant,
            value = true,
            tenantId = otherTenantId,
        }, TestContext.Current.CancellationToken);

        putResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Features_ShouldOnlyListCurrentTenantValuesFromTenantContext()
    {
        await using var app = await CreateApplicationAsync();
        var currentTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        await SeedTenantsAsync(app, currentTenantId, otherTenantId);
        await SeedFeatureAsync(app, "workspace.current-feature", currentTenantId);
        await SeedFeatureAsync(app, "workspace.other-feature", otherTenantId);

        var client = await CreateAuthenticatedTenantClientAsync(app, currentTenantId);

        var listResponse = await client.GetAsync("/api/v1/features", TestContext.Current.CancellationToken);
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var list = await listResponse.Content.ReadFromJsonAsync<ItemsWrapper<FeatureDto>>(TestContext.Current.CancellationToken);
        list.ShouldNotBeNull();
        list.Items.ShouldContain(feature => feature.Name == "workspace.current-feature" && feature.TenantId == currentTenantId);
        list.Items.ShouldNotContain(feature => feature.Name == "workspace.other-feature" && feature.TenantId == otherTenantId);
    }

    [Fact]
    public async Task Features_ShouldForbidTenantTokenWithAnotherTenantHeader()
    {
        await using var app = await CreateApplicationAsync();
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        await SeedTenantsAsync(app, tenantId, otherTenantId);

        var client = await CreateAuthenticatedTenantClientAsync(app, tenantId, otherTenantId);

        var listResponse = await client.GetAsync("/api/v1/features", TestContext.Current.CancellationToken);

        listResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Permissions_ShouldForbidWritingAnotherTenantGrantFromTenantContext()
    {
        await using var app = await CreateApplicationAsync();
        var currentTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        await SeedTenantsAsync(app, currentTenantId, otherTenantId);

        var client = await CreateAuthenticatedTenantClientAsync(app, currentTenantId);

        var putResponse = await client.PutAsJsonAsync("/api/v1/permissions", new
        {
            name = "workspace.cross-tenant-permission",
            scope = PermissionScope.Tenant,
            isGranted = true,
            tenantId = otherTenantId,
        }, TestContext.Current.CancellationToken);

        putResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Permissions_ShouldOnlyListCurrentTenantGrantsFromTenantContext()
    {
        await using var app = await CreateApplicationAsync();
        var currentTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        await SeedTenantsAsync(app, currentTenantId, otherTenantId);
        await SeedPermissionAsync(app, "workspace.current-permission", currentTenantId);
        await SeedPermissionAsync(app, "workspace.other-permission", otherTenantId);

        var client = await CreateAuthenticatedTenantClientAsync(app, currentTenantId);

        var listResponse = await client.GetAsync("/api/v1/permissions", TestContext.Current.CancellationToken);
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var list = await listResponse.Content.ReadFromJsonAsync<ItemsWrapper<PermissionDto>>(TestContext.Current.CancellationToken);
        list.ShouldNotBeNull();
        list.Items.ShouldContain(permission => permission.Name == "workspace.current-permission" && permission.TenantId == currentTenantId);
        list.Items.ShouldNotContain(permission => permission.Name == "workspace.other-permission" && permission.TenantId == otherTenantId);
    }

    [Fact]
    public async Task Permissions_ShouldForbidTenantTokenWithAnotherTenantHeader()
    {
        await using var app = await CreateApplicationAsync();
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        await SeedTenantsAsync(app, tenantId, otherTenantId);

        var client = await CreateAuthenticatedTenantClientAsync(app, tenantId, otherTenantId);

        var listResponse = await client.GetAsync("/api/v1/permissions", TestContext.Current.CancellationToken);

        listResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private static async Task<WebApplication> CreateApplicationAsync()
    {
        var databaseName = $"management-scope-web-{Guid.NewGuid():N}";
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

    private static async Task SeedFeatureAsync(WebApplication app, string name, Guid tenantId)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FeatureManagementDbContext>();
        await dbContext.FeatureValues.AddAsync(
            new FeatureValueEntity(Guid.NewGuid(), name, FeatureScope.Tenant, true, tenantId),
            TestContext.Current.CancellationToken);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static async Task SeedPermissionAsync(WebApplication app, string name, Guid tenantId)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PermissionManagementDbContext>();
        await dbContext.PermissionGrants.AddAsync(
            new PermissionGrantEntity(Guid.NewGuid(), name, PermissionScope.Tenant, true, tenantId),
            TestContext.Current.CancellationToken);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private sealed class FeatureDto
    {
        public string Name { get; set; } = string.Empty;

        public Guid? TenantId { get; set; }
    }

    private sealed class PermissionDto
    {
        public string Name { get; set; } = string.Empty;

        public Guid? TenantId { get; set; }
    }

    private sealed class ItemsWrapper<T>
    {
        public List<T> Items { get; set; } = [];
    }
}
