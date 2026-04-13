using System.Net;
using System.Net.Http.Json;
using ChengYuan.Core.Data;
using ChengYuan.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace ChengYuan.FrameworkKernel.Tests.Hosts.WebHost;

public sealed class IdentityEndpointsTests
{
    private static IHostBuilder CreateHostBuilder() => new HostBuilder()
        .ConfigureWebHost(webBuilder =>
        {
            webBuilder.UseTestServer();
            webBuilder.ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddIdentityApplication();
                services.AddSingleton<InMemoryIdentityRoleRepository>();
                services.AddSingleton<InMemoryIdentityUserRepository>();
                services.AddSingleton<IIdentityRoleRepository>(sp => sp.GetRequiredService<InMemoryIdentityRoleRepository>());
                services.AddSingleton<IIdentityUserRepository>(sp => sp.GetRequiredService<InMemoryIdentityUserRepository>());
                services.AddSingleton<IUnitOfWork, NoopUnitOfWork>();
            });
            webBuilder.Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints => endpoints.MapIdentityManagementEndpoints());
            });
        });

    [Fact]
    public async Task Get_users_returns_200_with_empty_list()
    {
        using var host = await CreateHostBuilder().StartAsync(TestContext.Current.CancellationToken);
        var client = host.GetTestClient();

        var response = await client.GetAsync(new Uri("/identity/users", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_user_returns_201_and_location()
    {
        using var host = await CreateHostBuilder().StartAsync(TestContext.Current.CancellationToken);
        var client = host.GetTestClient();

        var response = await client.PostAsJsonAsync(
            new Uri("/identity/users", UriKind.Relative),
            new { userName = "testuser", email = "test@example.com" },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldStartWith("/identity/users/");
    }

    [Fact]
    public async Task Get_nonexistent_user_returns_404()
    {
        using var host = await CreateHostBuilder().StartAsync(TestContext.Current.CancellationToken);
        var client = host.GetTestClient();

        var response = await client.GetAsync(
            new Uri($"/identity/users/{Guid.NewGuid()}", UriKind.Relative),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_and_get_user_roundtrip()
    {
        using var host = await CreateHostBuilder().StartAsync(TestContext.Current.CancellationToken);
        var client = host.GetTestClient();

        var createResponse = await client.PostAsJsonAsync(
            new Uri("/identity/users", UriKind.Relative),
            new { userName = "roundtrip", email = "roundtrip@example.com" },
            TestContext.Current.CancellationToken);

        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var location = createResponse.Headers.Location!;

        var getResponse = await client.GetAsync(location, TestContext.Current.CancellationToken);

        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_roles_returns_200()
    {
        using var host = await CreateHostBuilder().StartAsync(TestContext.Current.CancellationToken);
        var client = host.GetTestClient();

        var response = await client.GetAsync(new Uri("/identity/roles", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_role_returns_201()
    {
        using var host = await CreateHostBuilder().StartAsync(TestContext.Current.CancellationToken);
        var client = host.GetTestClient();

        var response = await client.PostAsJsonAsync(
            new Uri("/identity/roles", UriKind.Relative),
            new { name = "admin" },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task Delete_user_returns_204()
    {
        using var host = await CreateHostBuilder().StartAsync(TestContext.Current.CancellationToken);
        var client = host.GetTestClient();

        var createResponse = await client.PostAsJsonAsync(
            new Uri("/identity/users", UriKind.Relative),
            new { userName = "todelete", email = "del@example.com" },
            TestContext.Current.CancellationToken);

        var location = createResponse.Headers.Location!;

        var deleteResponse = await client.DeleteAsync(location, TestContext.Current.CancellationToken);

        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}
