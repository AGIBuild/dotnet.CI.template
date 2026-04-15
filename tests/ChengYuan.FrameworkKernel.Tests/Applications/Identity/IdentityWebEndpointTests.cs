using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChengYuan.Core.Modularity;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.Identity;
using ChengYuan.MultiTenancy;
using ChengYuan.WebHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class IdentityWebEndpointTests
{
    [Fact]
    public async Task IdentityWeb_ShouldManageUsersRolesAndAssignmentsOverHttp()
    {
        await using var app = await CreateApplicationAsync();
        var client = await CreateAuthenticatedClientAsync(app);

        var createRoleResponse = await client.PostAsJsonAsync("/api/v1/identity/roles", new CreateRoleRequest
        {
            Name = "Administrators"
        }, TestContext.Current.CancellationToken);

        createRoleResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var createdRole = await createRoleResponse.Content.ReadFromJsonAsync<RoleRecord>(TestContext.Current.CancellationToken);
        createdRole.ShouldNotBeNull();

        var createUserResponse = await client.PostAsJsonAsync("/api/v1/identity/users", new CreateUserRequest
        {
            UserName = "alice",
            Email = "alice@example.com",
            Password = "AlicePass123!"
        }, TestContext.Current.CancellationToken);

        createUserResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var createdUser = await createUserResponse.Content.ReadFromJsonAsync<UserRecord>(TestContext.Current.CancellationToken);
        createdUser.ShouldNotBeNull();
        createdUser.RoleIds.ShouldBeEmpty();

        var assignRoleResponse = await client.PutAsync($"/api/v1/identity/users/{createdUser.Id}/roles/{createdRole.Id}", content: null, TestContext.Current.CancellationToken);
        assignRoleResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var assignedUser = await assignRoleResponse.Content.ReadFromJsonAsync<UserRecord>(TestContext.Current.CancellationToken);
        assignedUser.ShouldNotBeNull();
        assignedUser.RoleIds.ShouldBe([createdRole.Id]);

        var getUserResponse = await client.GetAsync($"/api/v1/identity/users/{createdUser.Id}", TestContext.Current.CancellationToken);
        getUserResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var loadedUser = await getUserResponse.Content.ReadFromJsonAsync<UserRecord>(TestContext.Current.CancellationToken);
        loadedUser.ShouldNotBeNull();
        loadedUser.RoleIds.ShouldBe([createdRole.Id]);

        var deleteRoleResponse = await client.DeleteAsync($"/api/v1/identity/roles/{createdRole.Id}", TestContext.Current.CancellationToken);
        deleteRoleResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getUserAfterRoleDeleteResponse = await client.GetAsync($"/api/v1/identity/users/{createdUser.Id}", TestContext.Current.CancellationToken);
        getUserAfterRoleDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var userAfterRoleDelete = await getUserAfterRoleDeleteResponse.Content.ReadFromJsonAsync<UserRecord>(TestContext.Current.CancellationToken);
        userAfterRoleDelete.ShouldNotBeNull();
        userAfterRoleDelete.RoleIds.ShouldBeEmpty();
    }

    [Fact]
    public async Task IdentityWeb_ShouldReturnBadRequestForDuplicateUsersAndRoles()
    {
        await using var app = await CreateApplicationAsync();
        var client = await CreateAuthenticatedClientAsync(app);

        var firstRoleResponse = await client.PostAsJsonAsync("/api/v1/identity/roles", new CreateRoleRequest
        {
            Name = "Administrators"
        }, TestContext.Current.CancellationToken);

        firstRoleResponse.EnsureSuccessStatusCode();

        var duplicateRoleResponse = await client.PostAsJsonAsync("/api/v1/identity/roles", new CreateRoleRequest
        {
            Name = " administrators "
        }, TestContext.Current.CancellationToken);

        duplicateRoleResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var firstUserResponse = await client.PostAsJsonAsync("/api/v1/identity/users", new CreateUserRequest
        {
            UserName = "alice",
            Email = "alice@example.com",
            Password = "AlicePass123!"
        }, TestContext.Current.CancellationToken);

        firstUserResponse.EnsureSuccessStatusCode();

        var duplicateUserResponse = await client.PostAsJsonAsync("/api/v1/identity/users", new CreateUserRequest
        {
            UserName = " ALICE ",
            Email = "other@example.com",
            Password = "AlicePass123!"
        }, TestContext.Current.CancellationToken);

        duplicateUserResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task IdentityWeb_ShouldLoginAndReturnAccessToken()
    {
        await using var app = await CreateApplicationAsync();

        var userManager = app.Services.GetRequiredService<IUserManager>();
        await userManager.CreateAsync("admin", "admin@test.com", "AdminPass123!", TestContext.Current.CancellationToken);

        var client = app.GetTestClient();
        var loginResponse = await client.PostAsJsonAsync("/api/v1/identity/login", new LoginRequest
        {
            UserName = "admin",
            Password = "AdminPass123!"
        }, TestContext.Current.CancellationToken);

        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var token = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>(TestContext.Current.CancellationToken);
        token.ShouldNotBeNull();
        token.AccessToken.ShouldNotBeNullOrWhiteSpace();
        token.ExpiresIn.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task IdentityWeb_ShouldReturnUnauthorizedForInvalidCredentials()
    {
        await using var app = await CreateApplicationAsync();

        var userManager = app.Services.GetRequiredService<IUserManager>();
        await userManager.CreateAsync("admin", "admin@test.com", "AdminPass123!", TestContext.Current.CancellationToken);

        var client = app.GetTestClient();
        var loginResponse = await client.PostAsJsonAsync("/api/v1/identity/login", new LoginRequest
        {
            UserName = "admin",
            Password = "WrongPassword!"
        }, TestContext.Current.CancellationToken);

        loginResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task IdentityWeb_ShouldReturnUnauthorizedForProtectedEndpointsWithoutToken()
    {
        await using var app = await CreateApplicationAsync();
        var client = app.GetTestClient();

        var response = await client.GetAsync("/api/v1/identity/users", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task IdentityWeb_ShouldExposeHealthEndpointAndModuleCatalog()
    {
        await using var app = await CreateApplicationAsync();
        var client = app.GetTestClient();

        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<HealthPayload>(TestContext.Current.CancellationToken);
        payload.ShouldNotBeNull();
        payload.Status.ShouldBe("ok");
        payload.Modules.ShouldContain(nameof(IdentityWebModule));
        payload.Modules.ShouldContain("WebHostHttpCompositionModule");
    }

    private static async Task<WebApplication> CreateApplicationAsync()
    {
        var databaseName = $"identity-web-{Guid.NewGuid():N}";
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

    private sealed class HealthPayload
    {
        public string Status { get; init; } = string.Empty;

        public string CorrelationId { get; init; } = string.Empty;

        public Guid? TenantId { get; init; }

        public IReadOnlyList<string> Modules { get; init; } = Array.Empty<string>();
    }
}
