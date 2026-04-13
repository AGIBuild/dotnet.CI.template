using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using ChengYuan.Core.Modularity;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.MultiTenancy;
using ChengYuan.WebHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class TenantResolutionMiddlewareTests
{
    [Fact]
    public async Task Middleware_ShouldResolveTenantFromHeader()
    {
        var tenantId = Guid.NewGuid();

        await using var app = await CreateApplicationAsync();
        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<HealthPayload>(TestContext.Current.CancellationToken);
        payload.ShouldNotBeNull();
        payload.TenantId.ShouldBe(tenantId);
    }

    [Fact]
    public async Task Middleware_ShouldPreferClaimOverHeader()
    {
        var claimTenantId = Guid.NewGuid();
        var headerTenantId = Guid.NewGuid();

        await using var app = await CreateApplicationAsync(beforeMultiTenancy: webApp =>
        {
            webApp.Use((context, next) =>
            {
                context.User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim("tenant_id", claimTenantId.ToString())
                ],
                authenticationType: "test"));

                return next(context);
            });
        });

        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", headerTenantId.ToString());

        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<HealthPayload>(TestContext.Current.CancellationToken);
        payload.ShouldNotBeNull();
        payload.TenantId.ShouldBe(claimTenantId);
    }

    [Fact]
    public async Task Middleware_ShouldReturnBadRequest_WhenTenantIsRequiredButUnresolved()
    {
        await using var app = await CreateApplicationAsync(configure: builder => builder.RequireResolvedTenant());
        var client = app.GetTestClient();

        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var message = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        message.ShouldBe("Tenant could not be resolved.");
    }

    [Fact]
    public async Task Middleware_ShouldAllowRequest_WhenTenantIsOptionalAndUnresolved()
    {
        await using var app = await CreateApplicationAsync();
        var client = app.GetTestClient();

        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<HealthPayload>(TestContext.Current.CancellationToken);
        payload.ShouldNotBeNull();
        payload.TenantId.ShouldBeNull();
    }

    [Fact]
    public async Task Middleware_ShouldResolveTenantFromQueryString()
    {
        var tenantId = Guid.NewGuid();

        await using var app = await CreateApplicationAsync();
        var client = app.GetTestClient();

        var response = await client.GetAsync($"/health?__tenant={tenantId}", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<HealthPayload>(TestContext.Current.CancellationToken);
        payload.ShouldNotBeNull();
        payload.TenantId.ShouldBe(tenantId);
    }

    [Fact]
    public async Task Middleware_ShouldResolveTenantFromCookie()
    {
        var tenantId = Guid.NewGuid();

        await using var app = await CreateApplicationAsync();
        var client = app.GetTestClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Cookie", $"__tenant={tenantId}");

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<HealthPayload>(TestContext.Current.CancellationToken);
        payload.ShouldNotBeNull();
        payload.TenantId.ShouldBe(tenantId);
    }

    [Fact]
    public async Task Middleware_ShouldResolveTenantFromDomainPattern()
    {
        var tenantId = Guid.NewGuid();

        await using var app = await CreateApplicationAsync(
            configure: builder => builder.UseDomain("{0}.myapp.com"),
            configureServices: services =>
            {
                // Register a store that resolves "acme" to a known tenant id
                services.AddSingleton<ITenantResolutionStore>(
                    new InMemoryTenantResolutionStore(
                        new TenantResolutionRecord(tenantId, "acme", true)));
            });

        var client = app.GetTestClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Host = "acme.myapp.com";

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<HealthPayload>(TestContext.Current.CancellationToken);
        payload.ShouldNotBeNull();
        payload.TenantId.ShouldBe(tenantId);
    }

    [Fact]
    public async Task Middleware_ShouldReturnNotFound_WhenTenantNameNotInStore()
    {
        await using var app = await CreateApplicationAsync(
            configureServices: services =>
            {
                services.AddSingleton<ITenantResolutionStore>(
                    new InMemoryTenantResolutionStore());
            });

        var client = app.GetTestClient();
        // Send a non-Guid header value that triggers name-based lookup
        client.DefaultRequestHeaders.Add("X-Tenant-Id", "unknown-tenant");

        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Middleware_ShouldReturnForbidden_WhenTenantIsInactive()
    {
        var tenantId = Guid.NewGuid();

        await using var app = await CreateApplicationAsync(
            configureServices: services =>
            {
                services.AddSingleton<ITenantResolutionStore>(
                    new InMemoryTenantResolutionStore(
                        new TenantResolutionRecord(tenantId, "disabled-corp", false)));
            });

        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", "disabled-corp");

        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Middleware_ShouldPreferHeaderOverQueryString()
    {
        var headerTenantId = Guid.NewGuid();
        var queryTenantId = Guid.NewGuid();

        await using var app = await CreateApplicationAsync();
        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", headerTenantId.ToString());

        var response = await client.GetAsync($"/health?__tenant={queryTenantId}", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<HealthPayload>(TestContext.Current.CancellationToken);
        payload.ShouldNotBeNull();
        payload.TenantId.ShouldBe(headerTenantId);
    }

    [Fact]
    public async Task Middleware_ShouldInvokeCustomErrorHandler()
    {
        await using var app = await CreateApplicationAsync(
            configure: builder => builder
                .RequireResolvedTenant()
                .ConfigureErrorHandler(async (context, result) =>
                {
                    context.Response.StatusCode = 418;
                    await context.Response.WriteAsync("custom-error");
                    return true;
                }));

        var client = app.GetTestClient();

        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe((HttpStatusCode)418);
        var message = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        message.ShouldBe("custom-error");
    }

    private static async Task<WebApplication> CreateApplicationAsync(
        Action<MultiTenancyBuilder>? configure = null,
        Action<WebApplication>? beforeMultiTenancy = null,
        Action<IServiceCollection>? configureServices = null)
    {
        var databaseName = $"tenant-middleware-{Guid.NewGuid():N}";
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.UseDbContextOptions(options => options.UseInMemoryDatabase(databaseName));
        builder.AddTestWebHost(configure);
        configureServices?.Invoke(builder.Services);

        var app = builder.Build();
        beforeMultiTenancy?.Invoke(app);
        app.UseWebHostComposition();
        await app.StartAsync();

        return app;
    }

    private sealed class HealthPayload
    {
        public string Status { get; init; } = string.Empty;

        public string CorrelationId { get; init; } = string.Empty;

        public Guid? TenantId { get; init; }

        public IReadOnlyList<string> Modules { get; init; } = Array.Empty<string>();
    }

    private sealed class InMemoryTenantResolutionStore(
        params TenantResolutionRecord[] records) : ITenantResolutionStore
    {
        public Task<TenantResolutionRecord?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(records.FirstOrDefault(r => r.Id == id));

        public Task<TenantResolutionRecord?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
            => Task.FromResult(records.FirstOrDefault(r =>
                string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase)));
    }
}
