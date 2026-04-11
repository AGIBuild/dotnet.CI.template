using System;
using ChengYuan.Core.Data;
using ChengYuan.Core.Modularity;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class MultiTenancyRuntimeTests
{
    [Fact]
    public void CurrentTenantAccessor_ShouldStartWithNullScope()
    {
        var services = new ServiceCollection();
        services.AddModule<MultiTenancyTestModule>();
        using var serviceProvider = services.BuildServiceProvider();

        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenant>();

        currentTenant.Id.ShouldBeNull();
        currentTenant.Name.ShouldBeNull();
        currentTenant.IsAvailable.ShouldBeFalse();
    }

    [Fact]
    public void CurrentTenantAccessor_ShouldChangeScope()
    {
        var services = new ServiceCollection();
        services.AddModule<MultiTenancyTestModule>();
        using var serviceProvider = services.BuildServiceProvider();

        var accessor = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var tenantId = Guid.NewGuid();

        using (accessor.Change(tenantId, "test-tenant"))
        {
            accessor.Id.ShouldBe(tenantId);
            accessor.Name.ShouldBe("test-tenant");
            accessor.IsAvailable.ShouldBeTrue();
        }

        accessor.Id.ShouldBeNull();
        accessor.IsAvailable.ShouldBeFalse();
    }

    [Fact]
    public void CurrentTenantAccessor_ShouldRestorePreviousScope_WhenNestedScopesEnd()
    {
        var services = new ServiceCollection();
        services.AddModule<MultiTenancyTestModule>();
        using var serviceProvider = services.BuildServiceProvider();

        var accessor = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var outerTenantId = Guid.NewGuid();
        var innerTenantId = Guid.NewGuid();

        using (accessor.Change(outerTenantId, "outer"))
        {
            accessor.Id.ShouldBe(outerTenantId);
            accessor.Name.ShouldBe("outer");

            using (accessor.Change(innerTenantId, "inner"))
            {
                accessor.Id.ShouldBe(innerTenantId);
                accessor.Name.ShouldBe("inner");
            }

            accessor.Id.ShouldBe(outerTenantId);
            accessor.Name.ShouldBe("outer");
        }

        accessor.Id.ShouldBeNull();
        accessor.IsAvailable.ShouldBeFalse();
    }

    [Fact]
    public void CurrentTenantAccessor_ShouldHandleNullScopeAsHostScope()
    {
        var services = new ServiceCollection();
        services.AddModule<MultiTenancyTestModule>();
        using var serviceProvider = services.BuildServiceProvider();

        var accessor = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var tenantId = Guid.NewGuid();

        using (accessor.Change(tenantId, "tenant"))
        {
            using (accessor.Change(null, null))
            {
                accessor.Id.ShouldBeNull();
                accessor.IsAvailable.ShouldBeFalse();
            }

            accessor.Id.ShouldBe(tenantId);
            accessor.IsAvailable.ShouldBeTrue();
        }
    }

    [Fact]
    public void DataTenantProvider_ShouldReflectActiveScope()
    {
        var services = new ServiceCollection();
        services.AddModule<MultiTenancyTestModule>();
        using var serviceProvider = services.BuildServiceProvider();

        var accessor = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var dataTenantProvider = serviceProvider.GetRequiredService<IDataTenantProvider>();
        var tenantId = Guid.NewGuid();

        dataTenantProvider.TenantId.ShouldBeNull();
        dataTenantProvider.IsAvailable.ShouldBeFalse();

        using (accessor.Change(tenantId))
        {
            dataTenantProvider.TenantId.ShouldBe(tenantId);
            dataTenantProvider.IsAvailable.ShouldBeTrue();
        }

        dataTenantProvider.TenantId.ShouldBeNull();
        dataTenantProvider.IsAvailable.ShouldBeFalse();
    }

    [Fact]
    public void CurrentTenant_ShouldBeSameInstanceAsAccessor()
    {
        var services = new ServiceCollection();
        services.AddModule<MultiTenancyTestModule>();
        using var serviceProvider = services.BuildServiceProvider();

        var accessor = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenant>();

        currentTenant.ShouldBeSameAs(accessor);
    }

    [DependsOn(typeof(MultiTenancyModule))]
    private sealed class MultiTenancyTestModule : ModuleBase;
}
