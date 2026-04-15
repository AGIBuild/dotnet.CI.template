using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.Identity;
using ChengYuan.WebHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class TenantManagementWebEndpointTests
{
    [Fact]
    public async Task Tenants_ShouldReturnUnauthorizedWithoutToken()
    {
        await using var app = await CreateApplicationAsync();
        var client = app.GetTestClient();

        var response = await client.GetAsync("/api/v1/tenants", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Tenants_ShouldCreateAndListTenants()
    {
        await using var app = await CreateApplicationAsync();
        var client = await CreateAuthenticatedClientAsync(app);

        var createResponse = await client.PostAsJsonAsync("/api/v1/tenants", new
        {
            name = "Tenant-A",
            isActive = true,
        }, TestContext.Current.CancellationToken);

        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        createResponse.Headers.Location.ShouldNotBeNull();

        var created = await createResponse.Content.ReadFromJsonAsync<TenantDto>(TestContext.Current.CancellationToken);
        created.ShouldNotBeNull();
        created.Name.ShouldBe("Tenant-A");
        created.IsActive.ShouldBeTrue();

        var listResponse = await client.GetAsync("/api/v1/tenants", TestContext.Current.CancellationToken);
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var list = await listResponse.Content.ReadFromJsonAsync<ItemsWrapper<TenantDto>>(TestContext.Current.CancellationToken);
        list.ShouldNotBeNull();
        list.Items.Count.ShouldBeGreaterThanOrEqualTo(1);
        list.Items.ShouldContain(t => t.Name == "Tenant-A");
    }

    [Fact]
    public async Task Tenants_ShouldGetTenantByIdAndReturn404ForNonexistent()
    {
        await using var app = await CreateApplicationAsync();
        var client = await CreateAuthenticatedClientAsync(app);

        var createResponse = await client.PostAsJsonAsync("/api/v1/tenants", new
        {
            name = "Lookup-Tenant",
        }, TestContext.Current.CancellationToken);

        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<TenantDto>(TestContext.Current.CancellationToken);

        var getResponse = await client.GetAsync($"/api/v1/tenants/{created!.Id}", TestContext.Current.CancellationToken);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var loaded = await getResponse.Content.ReadFromJsonAsync<TenantDto>(TestContext.Current.CancellationToken);
        loaded.ShouldNotBeNull();
        loaded.Id.ShouldBe(created.Id);
        loaded.Name.ShouldBe("Lookup-Tenant");

        var notFoundResponse = await client.GetAsync($"/api/v1/tenants/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        notFoundResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Tenants_ShouldUpdateTenantViaPut()
    {
        await using var app = await CreateApplicationAsync();
        var client = await CreateAuthenticatedClientAsync(app);

        var createResponse = await client.PostAsJsonAsync("/api/v1/tenants", new
        {
            name = "Before-Update",
        }, TestContext.Current.CancellationToken);

        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<TenantDto>(TestContext.Current.CancellationToken);

        var putResponse = await client.PutAsJsonAsync("/api/v1/tenants", new
        {
            id = created!.Id,
            name = "After-Update",
            isActive = false,
        }, TestContext.Current.CancellationToken);
        putResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/v1/tenants/{created.Id}", TestContext.Current.CancellationToken);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var loaded = await getResponse.Content.ReadFromJsonAsync<TenantDto>(TestContext.Current.CancellationToken);
        loaded.ShouldNotBeNull();
        loaded.Name.ShouldBe("After-Update");
        loaded.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Tenants_ShouldDeleteTenant()
    {
        await using var app = await CreateApplicationAsync();
        var client = await CreateAuthenticatedClientAsync(app);

        var createResponse = await client.PostAsJsonAsync("/api/v1/tenants", new
        {
            name = "To-Delete",
        }, TestContext.Current.CancellationToken);

        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<TenantDto>(TestContext.Current.CancellationToken);

        var deleteResponse = await client.DeleteAsync($"/api/v1/tenants/{created!.Id}", TestContext.Current.CancellationToken);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/v1/tenants/{created.Id}", TestContext.Current.CancellationToken);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Tenants_ShouldRejectDuplicateName()
    {
        await using var app = await CreateApplicationAsync();
        var client = await CreateAuthenticatedClientAsync(app);

        var firstResponse = await client.PostAsJsonAsync("/api/v1/tenants", new
        {
            name = "Unique-Tenant",
        }, TestContext.Current.CancellationToken);

        firstResponse.EnsureSuccessStatusCode();

        var duplicateResponse = await client.PostAsJsonAsync("/api/v1/tenants", new
        {
            name = "unique-tenant",
        }, TestContext.Current.CancellationToken);

        duplicateResponse.IsSuccessStatusCode.ShouldBeFalse();
    }

    private static async Task<WebApplication> CreateApplicationAsync()
    {
        var databaseName = $"tenant-web-{Guid.NewGuid():N}";
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

    private sealed class TenantDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    private sealed class ItemsWrapper<T>
    {
        public List<T> Items { get; set; } = [];
    }
}
