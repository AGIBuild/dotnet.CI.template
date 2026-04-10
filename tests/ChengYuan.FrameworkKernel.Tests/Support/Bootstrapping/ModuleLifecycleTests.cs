using ChengYuan.Core.Lifecycle;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class ModuleLifecycleTests
{
    [Fact]
    public async Task InitializeAsync_ShouldExecuteModulesInDependencyOrder()
    {
        var services = new ServiceCollection();
        services.AddModule<LifecycleRootModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var manager = serviceProvider.GetRequiredService<IModuleManager>();

        await manager.InitializeAsync(TestContext.Current.CancellationToken);

        LifecycleRecorder.InitLog.ShouldBe(
        [
            "LifecycleLeafModule.PreInitialize",
            "LifecycleLeafModule.Initialize",
            "LifecycleLeafModule.PostInitialize",
            "LifecycleMiddleModule.PreInitialize",
            "LifecycleMiddleModule.Initialize",
            "LifecycleMiddleModule.PostInitialize",
            "LifecycleRootModule.Initialize"
        ]);
    }

    [Fact]
    public async Task ShutdownAsync_ShouldExecuteModulesInReverseDependencyOrder()
    {
        var services = new ServiceCollection();
        services.AddModule<LifecycleRootModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var manager = serviceProvider.GetRequiredService<IModuleManager>();

        await manager.InitializeAsync(TestContext.Current.CancellationToken);
        LifecycleRecorder.Clear();

        await manager.ShutdownAsync(TestContext.Current.CancellationToken);

        LifecycleRecorder.ShutdownLog.ShouldBe(
        [
            "LifecycleRootModule.Shutdown",
            "LifecycleMiddleModule.Shutdown",
            "LifecycleLeafModule.Shutdown"
        ]);
    }

    [Fact]
    public async Task InitializeAsync_ShouldWrapExceptionsWithModuleName()
    {
        var services = new ServiceCollection();
        services.AddModule<FailingRootModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var manager = serviceProvider.GetRequiredService<IModuleManager>();

        var exception = await Should.ThrowAsync<InvalidOperationException>(manager.InitializeAsync(TestContext.Current.CancellationToken));

        exception.Message.ShouldContain("FailingModule");
        exception.InnerException.ShouldNotBeNull();
        exception.InnerException.Message.ShouldBe("Intentional failure");
    }

    private static class LifecycleRecorder
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

    private sealed class LifecycleLeafModule : ModuleBase,
        IOnModulePreInitialize, IOnModuleInitialize, IOnModulePostInitialize, IOnModuleShutdown
    {
        public Task PreInitializeAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.InitLog.Add("LifecycleLeafModule.PreInitialize");
            return Task.CompletedTask;
        }

        public Task InitializeAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.InitLog.Add("LifecycleLeafModule.Initialize");
            return Task.CompletedTask;
        }

        public Task PostInitializeAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.InitLog.Add("LifecycleLeafModule.PostInitialize");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync(IModuleShutdownContext context)
        {
            LifecycleRecorder.ShutdownLog.Add("LifecycleLeafModule.Shutdown");
            return Task.CompletedTask;
        }
    }

    [DependsOn(typeof(LifecycleLeafModule))]
    private sealed class LifecycleMiddleModule : ModuleBase,
        IOnModulePreInitialize, IOnModuleInitialize, IOnModulePostInitialize, IOnModuleShutdown
    {
        public Task PreInitializeAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.InitLog.Add("LifecycleMiddleModule.PreInitialize");
            return Task.CompletedTask;
        }

        public Task InitializeAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.InitLog.Add("LifecycleMiddleModule.Initialize");
            return Task.CompletedTask;
        }

        public Task PostInitializeAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.InitLog.Add("LifecycleMiddleModule.PostInitialize");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync(IModuleShutdownContext context)
        {
            LifecycleRecorder.ShutdownLog.Add("LifecycleMiddleModule.Shutdown");
            return Task.CompletedTask;
        }
    }

    [DependsOn(typeof(LifecycleMiddleModule))]
    private sealed class LifecycleRootModule : ModuleBase, IOnModuleInitialize, IOnModuleShutdown
    {
        public Task InitializeAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.InitLog.Add("LifecycleRootModule.Initialize");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync(IModuleShutdownContext context)
        {
            LifecycleRecorder.ShutdownLog.Add("LifecycleRootModule.Shutdown");
            return Task.CompletedTask;
        }
    }

    private sealed class FailingModule : ModuleBase, IOnModuleInitialize
    {
        public Task InitializeAsync(IModuleInitializationContext context)
        {
            throw new InvalidOperationException("Intentional failure");
        }
    }

    [DependsOn(typeof(FailingModule))]
    private sealed class FailingRootModule : ModuleBase;
}
