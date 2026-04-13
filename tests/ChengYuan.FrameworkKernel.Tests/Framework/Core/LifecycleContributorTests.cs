using ChengYuan.Core.Lifecycle;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class LifecycleContributorTests
{
    [Fact]
    public async Task Contributors_ShouldExecuteForEachModuleDuringInit()
    {
        var log = new List<string>();

        var services = new ServiceCollection();
        services.AddModule<ContributorTestRootModule>();
        services.AddSingleton(log);
        services.AddTransient<IModuleLifecycleContributor, TestInitContributor>();
        services.Configure<ModuleLifecycleOptions>(o => o.Contributors.Add(typeof(TestInitContributor)));

        using var provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<IModuleManager>();

        await manager.InitializeAsync(TestContext.Current.CancellationToken);

        log.ShouldContain("Init:ContributorTestLeafModule");
        log.ShouldContain("Init:ContributorTestRootModule");
        log.Count(s => s.StartsWith("Init:", StringComparison.Ordinal)).ShouldBe(2);
    }

    [Fact]
    public async Task Contributors_ShouldExecuteForEachModuleDuringShutdown()
    {
        var log = new List<string>();

        var services = new ServiceCollection();
        services.AddModule<ContributorTestRootModule>();
        services.AddSingleton(log);
        services.AddTransient<IModuleLifecycleContributor, TestShutdownContributor>();
        services.Configure<ModuleLifecycleOptions>(o => o.Contributors.Add(typeof(TestShutdownContributor)));

        using var provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<IModuleManager>();

        await manager.InitializeAsync(TestContext.Current.CancellationToken);
        await manager.ShutdownAsync(TestContext.Current.CancellationToken);

        log.ShouldContain("Shutdown:ContributorTestRootModule");
        log.ShouldContain("Shutdown:ContributorTestLeafModule");

        log.IndexOf("Shutdown:ContributorTestRootModule")
            .ShouldBeLessThan(log.IndexOf("Shutdown:ContributorTestLeafModule"));
    }

    [Fact]
    public async Task NoContributors_ShouldWorkNormally()
    {
        var services = new ServiceCollection();
        services.AddModule<ContributorTestRootModule>();

        using var provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<IModuleManager>();

        await manager.InitializeAsync(TestContext.Current.CancellationToken);
        await manager.ShutdownAsync(TestContext.Current.CancellationToken);
    }

    private sealed class ContributorTestLeafModule : ModuleBase;

    [DependsOn(typeof(ContributorTestLeafModule))]
    private sealed class ContributorTestRootModule : ModuleBase;

    private sealed class TestInitContributor(List<string> log) : IModuleLifecycleContributor
    {
        public Task InitializeAsync(IModuleInitializationContext context, IModuleDescriptor descriptor, ModuleBase instance)
        {
            log.Add($"Init:{descriptor.Name}");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync(IModuleShutdownContext context, IModuleDescriptor descriptor, ModuleBase instance)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class TestShutdownContributor(List<string> log) : IModuleLifecycleContributor
    {
        public Task InitializeAsync(IModuleInitializationContext context, IModuleDescriptor descriptor, ModuleBase instance)
        {
            return Task.CompletedTask;
        }

        public Task ShutdownAsync(IModuleShutdownContext context, IModuleDescriptor descriptor, ModuleBase instance)
        {
            log.Add($"Shutdown:{descriptor.Name}");
            return Task.CompletedTask;
        }
    }
}
