using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class ModuleCatalogTests
{
    [Fact]
    public void GetModule_ShouldReturnDescriptorForLoadedModule()
    {
        var catalog = BuildCatalog<CatalogRootModule>();

        var descriptor = catalog.GetModule(typeof(CatalogLeafModule));

        descriptor.ShouldNotBeNull();
        descriptor.ModuleType.ShouldBe(typeof(CatalogLeafModule));
        descriptor.Name.ShouldBe(nameof(CatalogLeafModule));
        descriptor.Assembly.ShouldBe(typeof(CatalogLeafModule).Assembly);
    }

    [Fact]
    public void GetModule_ShouldThrowForUnknownModule()
    {
        var catalog = BuildCatalog<CatalogLeafModule>();

        Should.Throw<InvalidOperationException>(() => catalog.GetModule(typeof(CatalogRootModule)));
    }

    [Fact]
    public void TryGetModule_ShouldReturnTrueForLoadedModule()
    {
        var catalog = BuildCatalog<CatalogRootModule>();

        catalog.TryGetModule(typeof(CatalogLeafModule), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.ModuleType.ShouldBe(typeof(CatalogLeafModule));
    }

    [Fact]
    public void TryGetModule_ShouldReturnFalseForUnknownModule()
    {
        var catalog = BuildCatalog<CatalogLeafModule>();

        catalog.TryGetModule(typeof(CatalogRootModule), out var descriptor).ShouldBeFalse();
        descriptor.ShouldBeNull();
    }

    [Fact]
    public void IsLoaded_ShouldReturnTrueForLoadedModule()
    {
        var catalog = BuildCatalog<CatalogRootModule>();

        catalog.IsLoaded<CatalogLeafModule>().ShouldBeTrue();
        catalog.IsLoaded<CatalogRootModule>().ShouldBeTrue();
    }

    [Fact]
    public void IsLoaded_ShouldReturnFalseForUnloadedModule()
    {
        var catalog = BuildCatalog<CatalogLeafModule>();

        catalog.IsLoaded<CatalogRootModule>().ShouldBeFalse();
    }

    [Fact]
    public void GetInitializationOrder_ShouldReturnDependenciesBeforeDependents()
    {
        var catalog = BuildCatalog<CatalogRootModule>();

        var order = catalog.GetInitializationOrder().Select(d => d.ModuleType).ToArray();

        Array.IndexOf(order, typeof(CatalogLeafModule))
            .ShouldBeLessThan(Array.IndexOf(order, typeof(CatalogRootModule)));
    }

    [Fact]
    public void GetShutdownOrder_ShouldReturnDependentsBeforeDependencies()
    {
        var catalog = BuildCatalog<CatalogRootModule>();

        var order = catalog.GetShutdownOrder().Select(d => d.ModuleType).ToArray();

        Array.IndexOf(order, typeof(CatalogRootModule))
            .ShouldBeLessThan(Array.IndexOf(order, typeof(CatalogLeafModule)));
    }

    [Fact]
    public void RootModule_ShouldHaveIsRootTrue()
    {
        var catalog = BuildCatalog<CatalogRootModule>();

        var root = catalog.GetModule(typeof(CatalogRootModule));
        var leaf = catalog.GetModule(typeof(CatalogLeafModule));

        root.IsRoot.ShouldBeTrue();
        leaf.IsRoot.ShouldBeFalse();
    }

    [Fact]
    public void Descriptor_ShouldExposeResolvedDependencies()
    {
        var catalog = BuildCatalog<CatalogRootModule>();

        var root = catalog.GetModule(typeof(CatalogRootModule));

        root.Dependencies.Count.ShouldBe(1);
        root.Dependencies[0].ModuleType.ShouldBe(typeof(CatalogLeafModule));
    }

    [Fact]
    public void IModuleCatalog_ShouldBeResolvableFromServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddModule<CatalogRootModule>();

        using var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetRequiredService<IModuleCatalog>().ShouldNotBeNull();
    }

    [Fact]
    public void IModuleManager_ShouldBeResolvableFromServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddModule<CatalogRootModule>();

        using var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetRequiredService<IModuleManager>().ShouldNotBeNull();
    }

    private static IModuleCatalog BuildCatalog<TModule>() where TModule : ModuleBase, new()
    {
        var services = new ServiceCollection();
        services.AddModule<TModule>();

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IModuleCatalog>();
    }

    private sealed class CatalogLeafModule : ModuleBase;

    [DependsOn(typeof(CatalogLeafModule))]
    private sealed class CatalogRootModule : ModuleBase;
}
