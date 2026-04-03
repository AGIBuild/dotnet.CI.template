using ChengYuan.Core.Data;
using ChengYuan.Core.EntityFrameworkCore;
using ChengYuan.Core.Modularity;
using ChengYuan.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class CoreDataModuleTests
{
    [Fact]
    public void DataModule_ShouldRegisterSoftDeleteDataFilter()
    {
        var services = new ServiceCollection();
        services.AddModule<DataTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var filter = serviceProvider.GetRequiredService<IDataFilter<SoftDeleteFilter>>();

        filter.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void DataModule_ShouldRegisterMultiTenantDataFilter()
    {
        var services = new ServiceCollection();
        services.AddModule<DataTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var filter = serviceProvider.GetRequiredService<IDataFilter<MultiTenantFilter>>();

        filter.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void DataModule_ShouldRegisterNullDataTenantProvider()
    {
        var services = new ServiceCollection();
        services.AddModule<DataTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var dataTenantProvider = serviceProvider.GetRequiredService<IDataTenantProvider>();

        dataTenantProvider.IsAvailable.ShouldBeFalse();
        dataTenantProvider.TenantId.ShouldBeNull();
    }

    [Fact]
    public void MultiTenancyModule_ShouldBridgeCurrentTenantIntoDataTenantProvider()
    {
        var services = new ServiceCollection();
        services.AddModule<MultiTenantDataTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var currentTenantAccessor = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var dataTenantProvider = serviceProvider.GetRequiredService<IDataTenantProvider>();
        var tenantId = Guid.NewGuid();

        dataTenantProvider.IsAvailable.ShouldBeFalse();
        dataTenantProvider.TenantId.ShouldBeNull();

        using (currentTenantAccessor.Change(tenantId, "tenant-a"))
        {
            dataTenantProvider.IsAvailable.ShouldBeTrue();
            dataTenantProvider.TenantId.ShouldBe(tenantId);
        }

        dataTenantProvider.IsAvailable.ShouldBeFalse();
        dataTenantProvider.TenantId.ShouldBeNull();
    }

    [Fact]
    public void DataFilter_ShouldRestorePreviousState_WhenNestedScopesEnd()
    {
        var services = new ServiceCollection();
        services.AddModule<DataTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var filter = serviceProvider.GetRequiredService<IDataFilter<SoftDeleteFilter>>();

        using (filter.Disable())
        {
            filter.IsEnabled.ShouldBeFalse();

            using (filter.Enable())
            {
                filter.IsEnabled.ShouldBeTrue();
            }

            filter.IsEnabled.ShouldBeFalse();
        }

        filter.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public void UnitOfWorkAccessor_ShouldRestorePreviousUnitOfWork_WhenScopeEnds()
    {
        var services = new ServiceCollection();
        services.AddModule<DataTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var unitOfWorkAccessor = serviceProvider.GetRequiredService<IUnitOfWorkAccessor>();
        var outerUnitOfWork = new TestUnitOfWork();
        var innerUnitOfWork = new TestUnitOfWork();

        unitOfWorkAccessor.Current.ShouldBeNull();

        using (unitOfWorkAccessor.Change(outerUnitOfWork))
        {
            unitOfWorkAccessor.Current.ShouldBeSameAs(outerUnitOfWork);

            using (unitOfWorkAccessor.Change(innerUnitOfWork))
            {
                unitOfWorkAccessor.Current.ShouldBeSameAs(innerUnitOfWork);
            }

            unitOfWorkAccessor.Current.ShouldBeSameAs(outerUnitOfWork);
        }

        unitOfWorkAccessor.Current.ShouldBeNull();
    }

    [Fact]
    public async Task DbContextUnitOfWork_ShouldDelegateSaveChangesAsyncToDbContext()
    {
        var dbContext = new RecordingDbContext();
        var unitOfWork = new DbContextUnitOfWork(dbContext);

        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        dbContext.SaveChangesAsyncCallCount.ShouldBe(1);
    }

    [DependsOn(typeof(DataModule))]
    private sealed class DataTestModule : ModuleBase
    {
    }

    [DependsOn(typeof(DataModule))]
    [DependsOn(typeof(MultiTenancyModule))]
    private sealed class MultiTenantDataTestModule : ModuleBase
    {
    }

    private sealed class TestUnitOfWork : IUnitOfWork
    {
        public ValueTask SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }
    }

    private sealed class RecordingDbContext : DbContext
    {
        public int SaveChangesAsyncCallCount { get; private set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesAsyncCallCount++;
            return Task.FromResult(1);
        }
    }
}
