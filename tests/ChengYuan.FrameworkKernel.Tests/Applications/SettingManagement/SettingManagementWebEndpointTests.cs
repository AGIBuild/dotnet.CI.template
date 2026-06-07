using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.Identity;
using ChengYuan.SettingManagement;
using ChengYuan.Settings;
using ChengYuan.TenantManagement;
using ChengYuan.WebHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class SettingManagementWebEndpointTests
{
    [Fact]
    public async Task Settings_ShouldReturnUnauthorizedWithoutToken()
    {
        await using var app = await CreateApplicationAsync();
        var client = app.GetTestClient();

        var response = await client.GetAsync("/api/v1/settings", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Settings_ShouldSetAndListSettings()
    {
        await using var app = await CreateApplicationAsync();
        var client = await CreateAuthenticatedClientAsync(app);

        var putResponse = await client.PutAsJsonAsync("/api/v1/settings", new
        {
            name = "app.theme",
            scope = 0,
            value = "dark",
        }, TestContext.Current.CancellationToken);
        putResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var listResponse = await client.GetAsync("/api/v1/settings", TestContext.Current.CancellationToken);
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var list = await listResponse.Content.ReadFromJsonAsync<ItemsWrapper<SettingDto>>(TestContext.Current.CancellationToken);
        list.ShouldNotBeNull();
        list.Items.ShouldContain(s => s.Name == "app.theme");
    }

    [Fact]
    public async Task Settings_ShouldDeleteSetting()
    {
        await using var app = await CreateApplicationAsync();
        var client = await CreateAuthenticatedClientAsync(app);

        var putResponse = await client.PutAsJsonAsync("/api/v1/settings", new
        {
            name = "app.language",
            scope = 0,
            value = "zh-CN",
        }, TestContext.Current.CancellationToken);
        putResponse.EnsureSuccessStatusCode();

        var deleteResponse = await client.DeleteAsync(
            "/api/v1/settings?name=app.language&scope=Global",
            TestContext.Current.CancellationToken);

        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var listResponse = await client.GetAsync("/api/v1/settings", TestContext.Current.CancellationToken);
        var list = await listResponse.Content.ReadFromJsonAsync<ItemsWrapper<SettingDto>>(TestContext.Current.CancellationToken);
        list.ShouldNotBeNull();
        list.Items.ShouldNotContain(s => s.Name == "app.language");
    }

    [Fact]
    public async Task Settings_ShouldRejectInvalidScope()
    {
        await using var app = await CreateApplicationAsync();
        var client = await CreateAuthenticatedClientAsync(app);

        var deleteResponse = await client.DeleteAsync(
            "/api/v1/settings?name=app.anything&scope=InvalidScope",
            TestContext.Current.CancellationToken);

        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Settings_ShouldSetTenantScopedSetting()
    {
        await using var app = await CreateApplicationAsync();
        var client = await CreateAuthenticatedClientAsync(app);

        var tenantId = Guid.NewGuid();
        var putResponse = await client.PutAsJsonAsync("/api/v1/settings", new
        {
            name = "app.color",
            scope = 1,
            value = "blue",
            tenantId,
        }, TestContext.Current.CancellationToken);
        putResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var listResponse = await client.GetAsync("/api/v1/settings", TestContext.Current.CancellationToken);
        var list = await listResponse.Content.ReadFromJsonAsync<ItemsWrapper<SettingDto>>(TestContext.Current.CancellationToken);
        list.ShouldNotBeNull();
        list.Items.ShouldContain(s => s.Name == "app.color" && s.TenantId == tenantId);
    }

    [Fact]
    public async Task Settings_ShouldForbidWritingAnotherTenantSettingFromTenantContext()
    {
        await using var app = await CreateApplicationAsync();
        var currentTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        await SeedTenantsAsync(app, currentTenantId, otherTenantId);

        var client = await CreateAuthenticatedTenantClientAsync(app, currentTenantId);

        var putResponse = await client.PutAsJsonAsync("/api/v1/settings", new
        {
            name = "app.cross-tenant",
            scope = SettingScope.Tenant,
            value = "blocked",
            tenantId = otherTenantId,
        }, TestContext.Current.CancellationToken);

        putResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Settings_ShouldOnlyListCurrentTenantValuesFromTenantContext()
    {
        await using var app = await CreateApplicationAsync();
        var currentTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        await SeedTenantsAsync(app, currentTenantId, otherTenantId);
        await SeedSettingAsync(app, "app.current", currentTenantId);
        await SeedSettingAsync(app, "app.other", otherTenantId);

        var client = await CreateAuthenticatedTenantClientAsync(app, currentTenantId);

        var listResponse = await client.GetAsync("/api/v1/settings", TestContext.Current.CancellationToken);
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var list = await listResponse.Content.ReadFromJsonAsync<ItemsWrapper<SettingDto>>(TestContext.Current.CancellationToken);
        list.ShouldNotBeNull();
        list.Items.ShouldContain(s => s.Name == "app.current" && s.TenantId == currentTenantId);
        list.Items.ShouldNotContain(s => s.Name == "app.other" && s.TenantId == otherTenantId);
    }

    [Fact]
    public async Task Settings_ShouldForbidTenantTokenWithAnotherTenantHeader()
    {
        await using var app = await CreateApplicationAsync();
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        await SeedTenantsAsync(app, tenantId, otherTenantId);

        var client = await CreateAuthenticatedTenantClientAsync(app, tenantId, otherTenantId);

        var listResponse = await client.GetAsync("/api/v1/settings", TestContext.Current.CancellationToken);

        listResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private static async Task<WebApplication> CreateApplicationAsync()
    {
        var databaseName = $"setting-web-{Guid.NewGuid():N}";
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.UseDbContextOptions(options => options.UseInMemoryDatabase(databaseName));
        builder.AddTestWebHost();

        var app = builder.Build();
        app.UseWebHostComposition();
        await app.StartAsync();

        return app;
    }

    private static async Task<HttpClient> CreateAuthenticatedClientAsync(WebApplication app)
    {
        var userManager = app.Services.GetRequiredService<IUserManager>();
        await userManager.CreateAsync("testadmin", "testadmin@test.com", "TestAdminPass123!", TestContext.Current.CancellationToken);

        var client = app.GetTestClient();
        var loginResponse = await client.PostAsJsonAsync("/api/v1/identity/login", new LoginRequest
        {
            UserName = "testadmin",
            Password = "TestAdminPass123!"
        }, TestContext.Current.CancellationToken);

        var token = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>(TestContext.Current.CancellationToken);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        return client;
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

    private static async Task SeedSettingAsync(WebApplication app, string name, Guid tenantId)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SettingManagementDbContext>();
        await dbContext.SettingValues.AddAsync(
            new SettingValueEntity(Guid.NewGuid(), name, SettingScope.Tenant, "seeded", tenantId),
            TestContext.Current.CancellationToken);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private sealed class SettingDto
    {
        public string Name { get; set; } = string.Empty;

        public string Scope { get; set; } = string.Empty;

        public object? Value { get; set; }

        public Guid? TenantId { get; set; }

        public string? UserId { get; set; }
    }

    private sealed class ItemsWrapper<T>
    {
        public List<T> Items { get; set; } = [];
    }
}
