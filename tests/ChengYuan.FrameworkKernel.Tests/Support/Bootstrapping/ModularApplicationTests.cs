using ChengYuan.Core.Lifecycle;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class ModularApplicationTests
{
    [Fact]
    public void IModularApplication_ShouldBeResolvableFromServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddModularApplication<ShellTestModule>();

        using var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetRequiredService<IModularApplication>().ShouldNotBeNull();
    }

    [Fact]
    public void ModularApplicationOptions_ShouldBeResolvableWithCorrectStartupModule()
    {
        var services = new ServiceCollection();
        services.AddModularApplication<ShellTestModule>(options => options.ApplicationName = "test-app");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<ModularApplicationOptions>();

        options.StartupModuleType.ShouldBe(typeof(ShellTestModule));
        options.ApplicationName.ShouldBe("test-app");
    }

    [Fact]
    public void Shell_ShouldExposeModuleCatalog()
    {
        var services = new ServiceCollection();
        services.AddModularApplication<ShellTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var application = serviceProvider.GetRequiredService<IModularApplication>();

        application.ModuleCatalog.ShouldNotBeNull();
        application.ModuleCatalog.IsLoaded<ShellTestModule>().ShouldBeTrue();
    }

    [Fact]
    public async Task InitializeAsync_ShouldDelegateToModuleManager()
    {
        var services = new ServiceCollection();
        services.AddModularApplication<ShellLifecycleModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var application = serviceProvider.GetRequiredService<IModularApplication>();

        await application.InitializeAsync(TestContext.Current.CancellationToken);

        ShellLifecycleRecorder.InitLog.ShouldContain("ShellLifecycleModule.Initialize");
    }

    [Fact]
    public async Task InitializeAsync_ShouldRejectDuplicateInitialization()
    {
        var services = new ServiceCollection();
        services.AddModularApplication<ShellTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var application = serviceProvider.GetRequiredService<IModularApplication>();

        await application.InitializeAsync(TestContext.Current.CancellationToken);

        var exception = await Should.ThrowAsync<InvalidOperationException>(application.InitializeAsync(TestContext.Current.CancellationToken));
        exception.Message.ShouldContain("already been initialized");
    }

    [Fact]
    public async Task ShutdownAsync_ShouldNoOpBeforeInitialization()
    {
        var services = new ServiceCollection();
        services.AddModularApplication<ShellTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var application = serviceProvider.GetRequiredService<IModularApplication>();

        await Should.NotThrowAsync(application.ShutdownAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ShutdownAsync_ShouldDelegateAfterInitialization()
    {
        var services = new ServiceCollection();
        services.AddModularApplication<ShellLifecycleModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var application = serviceProvider.GetRequiredService<IModularApplication>();

        await application.InitializeAsync(TestContext.Current.CancellationToken);
        ShellLifecycleRecorder.Clear();

        await application.ShutdownAsync(TestContext.Current.CancellationToken);

        ShellLifecycleRecorder.ShutdownLog.ShouldContain("ShellLifecycleModule.Shutdown");
    }

    [Fact]
    public async Task LifecycleContext_ShouldExposeCatalogAndCancellationToken()
    {
        var services = new ServiceCollection();
        services.AddModularApplication<ContextInspectingModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var application = serviceProvider.GetRequiredService<IModularApplication>();
        using var cts = new CancellationTokenSource();

        await application.InitializeAsync(cts.Token);

        ContextInspectingModule.CapturedCatalog.ShouldNotBeNull();
        ContextInspectingModule.CapturedToken.ShouldBe(cts.Token);
    }

    [Fact]
    public void ExistingContracts_ShouldStillBeResolvable()
    {
        var services = new ServiceCollection();
        services.AddModularApplication<ShellTestModule>();

        using var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetRequiredService<IModuleCatalog>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IModuleManager>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<ModuleCatalog>().ShouldNotBeNull();
    }

    private sealed class ShellTestModule : ModuleBase;

    private static class ShellLifecycleRecorder
    {
        [ThreadStatic]
        private static List<string>? _initLog;

        [ThreadStatic]
        private static List<string>? _shutdownLog;

        public static List<string> InitLog => _initLog ??= [];
        public static List<string> ShutdownLog => _shutdownLog ??= [];

        public static void Clear()
        {
            _initLog?.Clear();
            _shutdownLog?.Clear();
        }
    }

    private sealed class ShellLifecycleModule : ModuleBase, IOnModuleInitialize, IOnModuleShutdown
    {
        public Task InitializeAsync(IModuleInitializationContext context)
        {
            ShellLifecycleRecorder.InitLog.Add("ShellLifecycleModule.Initialize");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync(IModuleShutdownContext context)
        {
            ShellLifecycleRecorder.ShutdownLog.Add("ShellLifecycleModule.Shutdown");
            return Task.CompletedTask;
        }
    }

    private sealed class ContextInspectingModule : ModuleBase, IOnModuleInitialize
    {
        [ThreadStatic]
        internal static IModuleCatalog? CapturedCatalog;

        [ThreadStatic]
        internal static CancellationToken CapturedToken;

        public Task InitializeAsync(IModuleInitializationContext context)
        {
            CapturedCatalog = context.ModuleCatalog;
            CapturedToken = context.CancellationToken;
            return Task.CompletedTask;
        }
    }
}
