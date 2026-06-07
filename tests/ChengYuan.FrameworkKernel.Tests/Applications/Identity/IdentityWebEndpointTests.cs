using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using ChengYuan.Core.Modularity;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.Identity;
using ChengYuan.MultiTenancy;
using ChengYuan.TenantManagement;
using ChengYuan.WebHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
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
    public async Task IdentityWeb_ShouldReturnForbiddenForTenantLogin_WhenUserIsNotTenantMember()
    {
        await using var app = await CreateApplicationAsync();
        var tenantId = Guid.NewGuid();
        await SeedTenantAsync(app, tenantId);

        var userManager = app.Services.GetRequiredService<IUserManager>();
        await userManager.CreateAsync("admin", "admin@test.com", "AdminPass123!", TestContext.Current.CancellationToken);

        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

        var loginResponse = await client.PostAsJsonAsync("/api/v1/identity/login", new LoginRequest
        {
            UserName = "admin",
            Password = "AdminPass123!"
        }, TestContext.Current.CancellationToken);

        loginResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task IdentityWeb_ShouldIssueTenantToken_WhenUserIsTenantMember()
    {
        await using var app = await CreateApplicationAsync();
        var tenantId = Guid.NewGuid();
        await SeedTenantAsync(app, tenantId);

        var userManager = app.Services.GetRequiredService<IUserManager>();
        var membershipManager = app.Services.GetRequiredService<IUserTenantMembershipManager>();
        var user = await userManager.CreateAsync("admin", "admin@test.com", "AdminPass123!", TestContext.Current.CancellationToken);
        await membershipManager.AssignAsync(user.Id, tenantId, TestContext.Current.CancellationToken);

        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

        var loginResponse = await client.PostAsJsonAsync("/api/v1/identity/login", new LoginRequest
        {
            UserName = "admin",
            Password = "AdminPass123!"
        }, TestContext.Current.CancellationToken);

        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var token = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>(TestContext.Current.CancellationToken);
        token.ShouldNotBeNull();
        ReadClaimValue(token.AccessToken, "tenant_id").ShouldBe(tenantId.ToString());
    }

    [Fact]
    public async Task IdentityWeb_ShouldManageUserTenantMembershipsFromHostContext()
    {
        await using var app = await CreateApplicationAsync();
        var tenantId = Guid.NewGuid();
        await SeedTenantAsync(app, tenantId);
        var client = await CreateAuthenticatedClientAsync(app);

        var userManager = app.Services.GetRequiredService<IUserManager>();
        var user = await userManager.CreateAsync("tenantuser", "tenantuser@test.com", "TenantUserPass123!", TestContext.Current.CancellationToken);

        var assignResponse = await client.PutAsync(
            $"/api/v1/identity/users/{user.Id}/tenants/{tenantId}",
            content: null,
            TestContext.Current.CancellationToken);

        assignResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var assigned = await assignResponse.Content.ReadFromJsonAsync<UserTenantMembershipRecord>(TestContext.Current.CancellationToken);
        assigned.ShouldNotBeNull();
        assigned.UserId.ShouldBe(user.Id);
        assigned.TenantId.ShouldBe(tenantId);
        assigned.IsActive.ShouldBeTrue();

        var listResponse = await client.GetAsync(
            $"/api/v1/identity/users/{user.Id}/tenants",
            TestContext.Current.CancellationToken);
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var list = await listResponse.Content.ReadFromJsonAsync<ItemsWrapper<UserTenantMembershipRecord>>(TestContext.Current.CancellationToken);
        list.ShouldNotBeNull();
        list.Items.ShouldContain(membership => membership.UserId == user.Id && membership.TenantId == tenantId && membership.IsActive);

        var revokeResponse = await client.DeleteAsync(
            $"/api/v1/identity/users/{user.Id}/tenants/{tenantId}",
            TestContext.Current.CancellationToken);
        revokeResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task IdentityWeb_ShouldForbidTenantContextMembershipManagement()
    {
        await using var app = await CreateApplicationAsync();
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        await SeedTenantAsync(app, tenantId);
        await SeedTenantAsync(app, otherTenantId);

        var userManager = app.Services.GetRequiredService<IUserManager>();
        var membershipManager = app.Services.GetRequiredService<IUserTenantMembershipManager>();
        var user = await userManager.CreateAsync("tenantadmin", "tenantadmin@test.com", "TenantAdminPass123!", TestContext.Current.CancellationToken);
        await membershipManager.AssignAsync(user.Id, tenantId, TestContext.Current.CancellationToken);

        var client = await CreateTenantClientAsync(app, tenantId, "tenantadmin", "TenantAdminPass123!");

        var assignResponse = await client.PutAsync(
            $"/api/v1/identity/users/{user.Id}/tenants/{otherTenantId}",
            content: null,
            TestContext.Current.CancellationToken);

        assignResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task IdentityWeb_ShouldForbidTenantTokenFromIdentityManagementApis()
    {
        await using var app = await CreateApplicationAsync();
        var tenantId = Guid.NewGuid();
        await SeedTenantAsync(app, tenantId);

        var userManager = app.Services.GetRequiredService<IUserManager>();
        var membershipManager = app.Services.GetRequiredService<IUserTenantMembershipManager>();
        var user = await userManager.CreateAsync("tenantadmin", "tenantadmin@test.com", "TenantAdminPass123!", TestContext.Current.CancellationToken);
        await membershipManager.AssignAsync(user.Id, tenantId, TestContext.Current.CancellationToken);

        var client = await CreateTenantClientAsync(app, tenantId, "tenantadmin", "TenantAdminPass123!");

        var usersResponse = await client.GetAsync("/api/v1/identity/users", TestContext.Current.CancellationToken);
        var rolesResponse = await client.GetAsync("/api/v1/identity/roles", TestContext.Current.CancellationToken);

        usersResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        rolesResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task IdentityWeb_ShouldForbidOldTenantTokenAfterMembershipRevoked()
    {
        await using var app = await CreateApplicationAsync();
        var tenantId = Guid.NewGuid();
        await SeedTenantAsync(app, tenantId);

        var userManager = app.Services.GetRequiredService<IUserManager>();
        var membershipManager = app.Services.GetRequiredService<IUserTenantMembershipManager>();
        var user = await userManager.CreateAsync("tenantadmin", "tenantadmin@test.com", "TenantAdminPass123!", TestContext.Current.CancellationToken);
        await membershipManager.AssignAsync(user.Id, tenantId, TestContext.Current.CancellationToken);

        var client = await CreateTenantClientAsync(app, tenantId, "tenantadmin", "TenantAdminPass123!");
        await membershipManager.RevokeAsync(user.Id, tenantId, TestContext.Current.CancellationToken);

        var response = await client.GetAsync("/api/v1/settings", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task IdentityWeb_ShouldForbidOldTokenAfterUserIsDeactivated()
    {
        await using var app = await CreateApplicationAsync();
        var userManager = app.Services.GetRequiredService<IUserManager>();
        var user = await userManager.CreateAsync("sessionuser", "sessionuser@test.com", "SessionUserPass123!", TestContext.Current.CancellationToken);

        var client = app.GetTestClient();
        var loginResponse = await client.PostAsJsonAsync("/api/v1/identity/login", new LoginRequest
        {
            UserName = "sessionuser",
            Password = "SessionUserPass123!"
        }, TestContext.Current.CancellationToken);
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var token = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>(TestContext.Current.CancellationToken);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        await userManager.UpdateAsync(user.Id, "sessionuser", "sessionuser@test.com", isActive: false, TestContext.Current.CancellationToken);

        var response = await client.GetAsync("/api/v1/identity/users", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task IdentityWeb_ShouldForbidOldTokenAfterUserIsDeleted()
    {
        await using var app = await CreateApplicationAsync();
        var userManager = app.Services.GetRequiredService<IUserManager>();
        var user = await userManager.CreateAsync("deleteduser", "deleteduser@test.com", "DeletedUserPass123!", TestContext.Current.CancellationToken);

        var client = app.GetTestClient();
        var loginResponse = await client.PostAsJsonAsync("/api/v1/identity/login", new LoginRequest
        {
            UserName = "deleteduser",
            Password = "DeletedUserPass123!"
        }, TestContext.Current.CancellationToken);
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var token = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>(TestContext.Current.CancellationToken);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        await userManager.RemoveAsync(user.Id, TestContext.Current.CancellationToken);

        var response = await client.GetAsync("/api/v1/identity/users", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task IdentityWeb_ShouldRejectAssigningUserToUnknownTenant()
    {
        await using var app = await CreateApplicationAsync();
        var client = await CreateAuthenticatedClientAsync(app);

        var userManager = app.Services.GetRequiredService<IUserManager>();
        var user = await userManager.CreateAsync("tenantuser", "tenantuser@test.com", "TenantUserPass123!", TestContext.Current.CancellationToken);

        var assignResponse = await client.PutAsync(
            $"/api/v1/identity/users/{user.Id}/tenants/{Guid.NewGuid()}",
            content: null,
            TestContext.Current.CancellationToken);

        assignResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task IdentityWeb_ShouldRejectAssigningUserToInactiveTenant()
    {
        await using var app = await CreateApplicationAsync();
        var tenantId = Guid.NewGuid();
        await SeedTenantAsync(app, tenantId, isActive: false);
        var client = await CreateAuthenticatedClientAsync(app);

        var userManager = app.Services.GetRequiredService<IUserManager>();
        var user = await userManager.CreateAsync("tenantuser", "tenantuser@test.com", "TenantUserPass123!", TestContext.Current.CancellationToken);

        var assignResponse = await client.PutAsync(
            $"/api/v1/identity/users/{user.Id}/tenants/{tenantId}",
            content: null,
            TestContext.Current.CancellationToken);

        assignResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task IdentityWeb_ShouldForbidHostTokenWithTenantHeader()
    {
        await using var app = await CreateApplicationAsync();
        var tenantId = Guid.NewGuid();
        await SeedTenantAsync(app, tenantId);
        var client = await CreateAuthenticatedClientAsync(app);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

        var response = await client.GetAsync("/api/v1/identity/users", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task IdentityWeb_ShouldForbidTenantTokenWhenHeaderTargetsAnotherTenant()
    {
        await using var app = await CreateApplicationAsync();
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        await SeedTenantAsync(app, tenantId);
        await SeedTenantAsync(app, otherTenantId);

        var userManager = app.Services.GetRequiredService<IUserManager>();
        var membershipManager = app.Services.GetRequiredService<IUserTenantMembershipManager>();
        var user = await userManager.CreateAsync("tenantadmin", "tenantadmin@test.com", "TenantAdminPass123!", TestContext.Current.CancellationToken);
        await membershipManager.AssignAsync(user.Id, tenantId, TestContext.Current.CancellationToken);

        var client = await CreateTenantClientAsync(app, tenantId, "tenantadmin", "TenantAdminPass123!");
        client.DefaultRequestHeaders.Remove("X-Tenant-Id");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", otherTenantId.ToString());

        var response = await client.GetAsync("/api/v1/identity/users", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
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

    private static async Task<HttpClient> CreateTenantClientAsync(
        WebApplication app,
        Guid tenantId,
        string userName,
        string password)
    {
        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
        var loginResponse = await client.PostAsJsonAsync("/api/v1/identity/login", new LoginRequest
        {
            UserName = userName,
            Password = password
        }, TestContext.Current.CancellationToken);
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var token = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>(TestContext.Current.CancellationToken);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        return client;
    }

    private static async Task SeedTenantAsync(WebApplication app, Guid tenantId, bool isActive = true)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>();
        await dbContext.Tenants.AddAsync(
            new TenantEntity(tenantId, $"Tenant-{tenantId:N}", isActive),
            TestContext.Current.CancellationToken);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static string? ReadClaimValue(string accessToken, string claimType)
    {
        var token = new JsonWebTokenHandler().ReadJsonWebToken(accessToken);
        return token.Claims.SingleOrDefault(claim => claim.Type == claimType)?.Value;
    }

    private sealed class HealthPayload
    {
        public string Status { get; init; } = string.Empty;

        public string CorrelationId { get; init; } = string.Empty;

        public Guid? TenantId { get; init; }

        public IReadOnlyList<string> Modules { get; init; } = Array.Empty<string>();
    }

    private sealed class ItemsWrapper<T>
    {
        public List<T> Items { get; set; } = [];
    }
}
