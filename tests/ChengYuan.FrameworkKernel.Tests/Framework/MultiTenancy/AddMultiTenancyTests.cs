using System;
using System.Threading.Tasks;
using ChengYuan.Core.Data;
using ChengYuan.Core.Modularity;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class AddMultiTenancyTests
{
    [Fact]
    public void AddMultiTenancy_ShouldEnableMultiTenancy()
    {
        var services = new ServiceCollection();
        services.AddModule<TestModule>();
        services.AddMultiTenancy();

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<MultiTenancyOptions>();

        options.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void AddMultiTenancy_ShouldApplyBuilderConfiguration()
    {
        var fallbackId = Guid.NewGuid();

        var services = new ServiceCollection();
        services.AddModule<TestModule>();
        services.AddMultiTenancy(builder => builder
            .UseTenantKey("X-My-Tenant")
            .UseHeader("My-Header")
            .UseQueryString("q-tenant")
            .UseRoute("r-tenant")
            .UseCookie("c-tenant")
            .UseDomain("{0}.myapp.com")
            .UseFallback(fallbackId, "fallback-tenant")
            .RequireResolvedTenant());

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<MultiTenancyOptions>();

        options.TenantKey.ShouldBe("X-My-Tenant");
        options.HeaderName.ShouldBe("My-Header");
        options.QueryStringKey.ShouldBe("q-tenant");
        options.RouteKey.ShouldBe("r-tenant");
        options.CookieName.ShouldBe("c-tenant");
        options.DomainPatterns.ShouldContain("{0}.myapp.com");
        options.FallbackTenantId.ShouldBe(fallbackId);
        options.FallbackTenantName.ShouldBe("fallback-tenant");
        options.UnresolvedBehavior.ShouldBe(UnresolvedTenantBehavior.Fail);
    }

    [Fact]
    public void AddMultiTenancy_ShouldHaveSensibleDefaults()
    {
        var services = new ServiceCollection();
        services.AddModule<TestModule>();
        services.AddMultiTenancy();

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<MultiTenancyOptions>();

        options.TenantKey.ShouldBe("__tenant");
        options.HeaderName.ShouldBe("X-Tenant-Id");
        options.QueryStringKey.ShouldBe("__tenant");
        options.RouteKey.ShouldBe("__tenant");
        options.CookieName.ShouldBe("__tenant");
        options.DomainPatterns.ShouldBeEmpty();
        options.FallbackTenantId.ShouldBeNull();
        options.UnresolvedBehavior.ShouldBe(UnresolvedTenantBehavior.Continue);
        options.ClaimTypes.ShouldContain("tenant_id");
    }

    [Fact]
    public void AddMultiTenancy_ShouldRegisterResolver()
    {
        var services = new ServiceCollection();
        services.AddModule<TestModule>();
        services.AddMultiTenancy();

        using var serviceProvider = services.BuildServiceProvider();
        var resolver = serviceProvider.GetRequiredService<ITenantResolver>();

        resolver.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddMultiTenancy_ShouldApplyFallbackToResolver()
    {
        var fallbackId = Guid.NewGuid();

        var services = new ServiceCollection();
        services.AddModule<TestModule>();
        services.AddMultiTenancy(builder => builder
            .UseFallback(fallbackId, "fallback"));

        using var serviceProvider = services.BuildServiceProvider();
        var resolver = serviceProvider.GetRequiredService<ITenantResolver>();
        var result = await resolver.ResolveAsync(TestContext.Current.CancellationToken);

        result.TenantId.ShouldBe(fallbackId);
        result.TenantName.ShouldBe("fallback");
    }

    [Fact]
    public void AddMultiTenancy_ShouldRegisterDefaultResolutionStore()
    {
        var services = new ServiceCollection();
        services.AddModule<TestModule>();
        services.AddMultiTenancy();

        using var serviceProvider = services.BuildServiceProvider();
        var store = serviceProvider.GetRequiredService<ITenantResolutionStore>();

        store.ShouldNotBeNull();
    }

    [Fact]
    public void AddMultiTenancy_ShouldBridgeDataTenantProvider()
    {
        var services = new ServiceCollection();
        services.AddModule<TestModule>();
        services.AddMultiTenancy();

        using var serviceProvider = services.BuildServiceProvider();
        var accessor = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var dataTenantProvider = serviceProvider.GetRequiredService<IDataTenantProvider>();
        var tenantId = Guid.NewGuid();

        dataTenantProvider.IsAvailable.ShouldBeFalse();

        using (accessor.Change(tenantId, "test"))
        {
            dataTenantProvider.IsAvailable.ShouldBeTrue();
            dataTenantProvider.TenantId.ShouldBe(tenantId);
        }

        dataTenantProvider.IsAvailable.ShouldBeFalse();
    }

    [DependsOn(typeof(MultiTenancyModule))]
    private sealed class TestModule : FrameworkCoreModule;
}