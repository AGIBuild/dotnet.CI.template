using System.Security.Claims;
using ChengYuan.ExecutionContext;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace ChengYuan.FrameworkKernel.Tests.Hosts.WebHost;

public sealed class CurrentUserMiddlewareTests
{
    [Fact]
    public async Task Authenticated_user_is_bridged_to_ICurrentUser()
    {
        string? capturedUserId = null;
        string? capturedUserName = null;
        bool? capturedIsAuthenticated = null;

        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddExecutionContext();
                });
                webBuilder.Configure(app =>
                {
                    // Simulate authentication by setting HttpContext.User
                    app.Use((context, next) =>
                    {
                        context.User = new ClaimsPrincipal(new ClaimsIdentity(
                        [
                            new Claim("sub", "user-42"),
                            new Claim("name", "TestUser"),
                        ],
                        authenticationType: "test-scheme"));
                        return next(context);
                    });

                    app.UseMiddleware<ChengYuan.WebHost.CurrentUserMiddleware>();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                        endpoints.MapGet("/whoami", (ICurrentUser currentUser) =>
                        {
                            capturedUserId = currentUser.Id;
                            capturedUserName = currentUser.UserName;
                            capturedIsAuthenticated = currentUser.IsAuthenticated;
                            return Results.Ok();
                        }));
                });
            })
            .StartAsync(TestContext.Current.CancellationToken);

        var client = host.GetTestClient();
        await client.GetAsync(new Uri("/whoami", UriKind.Relative), TestContext.Current.CancellationToken);

        capturedUserId.ShouldBe("user-42");
        capturedUserName.ShouldBe("TestUser");
        capturedIsAuthenticated.ShouldBe(true);
    }

    [Fact]
    public async Task Anonymous_user_is_not_bridged()
    {
        bool? capturedIsAuthenticated = null;

        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddExecutionContext();
                });
                webBuilder.Configure(app =>
                {
                    app.UseMiddleware<ChengYuan.WebHost.CurrentUserMiddleware>();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                        endpoints.MapGet("/whoami", (ICurrentUser currentUser) =>
                        {
                            capturedIsAuthenticated = currentUser.IsAuthenticated;
                            return Results.Ok();
                        }));
                });
            })
            .StartAsync(TestContext.Current.CancellationToken);

        var client = host.GetTestClient();
        await client.GetAsync(new Uri("/whoami", UriKind.Relative), TestContext.Current.CancellationToken);

        capturedIsAuthenticated.ShouldBe(false);
    }
}
