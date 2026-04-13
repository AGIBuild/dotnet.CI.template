using ChengYuan.ExceptionHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace ChengYuan.FrameworkKernel.Tests.Hosts.WebHost;

public sealed class GlobalExceptionMiddlewareTests
{
    [Fact]
    public async Task Unhandled_exception_returns_500_json()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddExceptionHandling();
                });
                webBuilder.Configure(app =>
                {
                    app.UseMiddleware<ChengYuan.WebHost.GlobalExceptionMiddleware>();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                        endpoints.MapGet("/throws", () =>
                        {
                            throw new InvalidOperationException("test error");
#pragma warning disable CS0162 // Unreachable code detected
                            return Results.Ok();
#pragma warning restore CS0162
                        }));
                });
            })
            .StartAsync(TestContext.Current.CancellationToken);

        var client = host.GetTestClient();
        var response = await client.GetAsync(new Uri("/throws", UriKind.Relative), TestContext.Current.CancellationToken);

        ((int)response.StatusCode).ShouldBe(500);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
    }

    [Fact]
    public async Task Business_exception_returns_400()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddExceptionHandling();
                });
                webBuilder.Configure(app =>
                {
                    app.UseMiddleware<ChengYuan.WebHost.GlobalExceptionMiddleware>();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                        endpoints.MapGet("/business", () =>
                        {
                            throw new BusinessException("TEST", "business error");
#pragma warning disable CS0162
                            return Results.Ok();
#pragma warning restore CS0162
                        }));
                });
            })
            .StartAsync(TestContext.Current.CancellationToken);

        var client = host.GetTestClient();
        var response = await client.GetAsync(new Uri("/business", UriKind.Relative), TestContext.Current.CancellationToken);

        ((int)response.StatusCode).ShouldBe(400);
    }

    [Fact]
    public async Task UnauthorizedAccessException_returns_403()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddExceptionHandling();
                });
                webBuilder.Configure(app =>
                {
                    app.UseMiddleware<ChengYuan.WebHost.GlobalExceptionMiddleware>();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                        endpoints.MapGet("/forbidden", () =>
                        {
                            throw new UnauthorizedAccessException("access denied");
#pragma warning disable CS0162
                            return Results.Ok();
#pragma warning restore CS0162
                        }));
                });
            })
            .StartAsync(TestContext.Current.CancellationToken);

        var client = host.GetTestClient();
        var response = await client.GetAsync(new Uri("/forbidden", UriKind.Relative), TestContext.Current.CancellationToken);

        ((int)response.StatusCode).ShouldBe(403);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
    }
}
