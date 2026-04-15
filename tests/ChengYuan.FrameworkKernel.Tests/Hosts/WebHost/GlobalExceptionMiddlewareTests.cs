using System.Text.Json;
using ChengYuan.Core.Exceptions;
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
    public async Task Unhandled_exception_returns_500_problem_details()
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
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetInt32().ShouldBe(500);
        doc.RootElement.GetProperty("title").GetString().ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Business_exception_returns_400_with_error_code()
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
                            throw new BusinessException("business error", new ErrorCode("TEST"));
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
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetInt32().ShouldBe(400);
        doc.RootElement.GetProperty("title").GetString().ShouldBe("Business Rule Violation");
        doc.RootElement.GetProperty("detail").GetString().ShouldBe("business error");
        doc.RootElement.GetProperty("errorCode").GetString().ShouldBe("TEST");
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
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
    }

    [Fact]
    public async Task EntityNotFoundException_returns_404_with_entity_info()
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
                        endpoints.MapGet("/not-found", () =>
                        {
                            throw new EntityNotFoundException(typeof(string), 42);
#pragma warning disable CS0162
                            return Results.Ok();
#pragma warning restore CS0162
                        }));
                });
            })
            .StartAsync(TestContext.Current.CancellationToken);

        var client = host.GetTestClient();
        var response = await client.GetAsync(new Uri("/not-found", UriKind.Relative), TestContext.Current.CancellationToken);

        ((int)response.StatusCode).ShouldBe(404);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("status").GetInt32().ShouldBe(404);
        doc.RootElement.GetProperty("title").GetString().ShouldBe("Not Found");
        doc.RootElement.GetProperty("entityType").GetString().ShouldBe("System.String");
        doc.RootElement.GetProperty("entityId").GetString().ShouldBe("42");
    }
}
