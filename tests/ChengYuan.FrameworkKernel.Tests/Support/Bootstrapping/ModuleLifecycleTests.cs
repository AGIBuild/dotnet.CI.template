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

    [Fact]
    public void AddModule_ShouldExecuteOnLoadedInDependencyOrder()
    {
        LifecycleRecorder.Clear();

        var services = new ServiceCollection();

        services.AddModule<LoadRootModule>();

        LifecycleRecorder.LoadLog.ShouldBe(
        [
            "LoadLeafModule.Loaded:LoadMiddleModule",
            "LoadMiddleModule.Loaded:LoadRootModule",
            "LoadRootModule.Loaded:(none)"
        ]);
    }

    [Fact]
    public void AddModule_ShouldWrapLoadExceptionsWithModuleName()
    {
        var services = new ServiceCollection();

        var exception = Should.Throw<InvalidOperationException>(() => services.AddModule<FailingLoadRootModule>());

        exception.Message.ShouldContain(nameof(FailingLoadModule));
        exception.InnerException.ShouldNotBeNull();
        exception.InnerException.Message.ShouldBe("Load failure");
    }

    [Fact]
    public void AddModule_ShouldWrapServiceRegistrationExceptionsWithModuleName()
    {
        var services = new ServiceCollection();

        var exception = Should.Throw<InvalidOperationException>(() => services.AddModule<FailingConfigureRootModule>());

        exception.Message.ShouldContain(nameof(FailingConfigureModule));
        exception.InnerException.ShouldNotBeNull();
        exception.InnerException.Message.ShouldBe("Configure failure");
    }

    [Fact]
    public void FrameworkCoreModule_ShouldRejectApplicationDependencies()
    {
        var services = new ServiceCollection();

        var exception = Should.Throw<InvalidOperationException>(() => services.AddModule<InvalidFrameworkDependsOnApplicationModule>());

        exception.Message.ShouldContain(nameof(InvalidFrameworkDependsOnApplicationModule));
        exception.InnerException.ShouldNotBeNull();
        exception.InnerException.Message.ShouldContain(nameof(InvalidApplicationDependencyModule));
        exception.InnerException.Message.ShouldContain(nameof(ModuleCategory.FrameworkCore));
        exception.InnerException.Message.ShouldContain(nameof(ModuleCategory.Application));
    }

    [Fact]
    public void ApplicationModule_ShouldRejectExtensionDependencies()
    {
        var services = new ServiceCollection();

        var exception = Should.Throw<InvalidOperationException>(() => services.AddModule<InvalidApplicationDependsOnExtensionModule>());

        exception.Message.ShouldContain(nameof(InvalidApplicationDependsOnExtensionModule));
        exception.InnerException.ShouldNotBeNull();
        exception.InnerException.Message.ShouldContain(nameof(InvalidExtensionDependencyModule));
        exception.InnerException.Message.ShouldContain(nameof(ModuleCategory.Application));
        exception.InnerException.Message.ShouldContain(nameof(ModuleCategory.Extension));
    }

    [Fact]
    public void HostModule_ShouldRejectNonHostDependents()
    {
        var services = new ServiceCollection();

        var exception = Should.Throw<InvalidOperationException>(() => services.AddModule<InvalidExtensionDependsOnHostModule>());

        exception.Message.ShouldContain(nameof(InvalidHostDependencyModule));
        exception.InnerException.ShouldNotBeNull();
        exception.InnerException.Message.ShouldContain(nameof(InvalidExtensionDependsOnHostModule));
        exception.InnerException.Message.ShouldContain(nameof(ModuleCategory.Host));
        exception.InnerException.Message.ShouldContain(nameof(ModuleCategory.Extension));
    }

    [Fact]
    public async Task CategoryLifecycleHooks_ShouldExecuteInLifecycleOrder()
    {
        LifecycleRecorder.Clear();

        var services = new ServiceCollection();
        services.AddModule<HookedHostModule>();

        LifecycleRecorder.HookLog.ShouldBe([
            "HookedHostModule.Loaded"
        ]);

        using var serviceProvider = services.BuildServiceProvider();
        var manager = serviceProvider.GetRequiredService<IModuleManager>();

        await manager.InitializeAsync(TestContext.Current.CancellationToken);
        await manager.ShutdownAsync(TestContext.Current.CancellationToken);

        LifecycleRecorder.HookLog.ShouldBe([
            "HookedHostModule.Loaded",
            "HookedHostModule.PreInitializeHook",
            "HookedHostModule.InitializeHook",
            "HookedHostModule.PostInitializeHook",
            "HookedHostModule.ShutdownHook"
        ]);
    }

    [Fact]
    public void CategoryBaseHooks_ShouldExposeCachedTopologyState()
    {
        LifecycleRecorder.Clear();

        var services = new ServiceCollection();

        services.AddModule<TopologyAwareRootHostModule>();

        LifecycleRecorder.TopologyLog.ShouldBe([
            "Framework:TopologyAwareSharedFrameworkModule:FrameworkCore:0:1:False",
            "Application:TopologyAwareApplicationModule:False:True:True",
            "Extension:TopologyAwareExtensionModule:TopologyAwareApplicationModule:Application:True:False",
            "Host:TopologyAwareInnerHostModule:False",
            "Host:TopologyAwareRootHostModule:True"
        ]);
    }

    [Fact]
    public void FrameworkCoreModule_ShouldClassifyLeafFoundationFromDependents()
    {
        LifecycleRecorder.Clear();

        var services = new ServiceCollection();

        services.AddModule<LeafFoundationRootHostModule>();

        LifecycleRecorder.TopologyLog.ShouldContain("FrameworkLeaf:LeafFoundationModule:True");
    }

    [Fact]
    public void ExtensionModule_ShouldResolveAttachedCapabilityAcrossSupportedGraphs()
    {
        LifecycleRecorder.Clear();

        var services = new ServiceCollection();

        services.AddModule<ExtensionAttachmentRootModule>();

        LifecycleRecorder.AttachmentLog.ShouldBe([
            "ApplicationBoundExtensionModule:ApplicationCapabilityModule:Application:True:False",
            "ChainedApplicationExtensionModule:ApplicationCapabilityModule:Application:True:False",
            "FrameworkBoundExtensionModule:FrameworkCapabilityModule:FrameworkCore:False:True"
        ]);
    }

    [Fact]
    public void ExtensionModule_ShouldRejectAmbiguousAttachmentGraphs()
    {
        var services = new ServiceCollection();

        var exception = Should.Throw<InvalidOperationException>(() => services.AddModule<AmbiguousExtensionRootModule>());

        exception.Message.ShouldContain(nameof(AmbiguousAttachmentExtensionModule));
        exception.InnerException.ShouldNotBeNull();
        exception.InnerException.Message.ShouldContain("ambiguous");
        exception.InnerException.Message.ShouldContain(nameof(FirstApplicationCapabilityModule));
        exception.InnerException.Message.ShouldContain(nameof(SecondApplicationCapabilityModule));
    }

    [Fact]
    public async Task HostModule_ShouldBranchBetweenRootAndInnerCompositionHooks()
    {
        LifecycleRecorder.Clear();

        var services = new ServiceCollection();
        services.AddModule<BranchingRootHostModule>();

        LifecycleRecorder.HostBranchLog.ShouldBe([
            "InnerHost.Loaded",
            "RootHost.Loaded"
        ]);

        using var serviceProvider = services.BuildServiceProvider();
        var manager = serviceProvider.GetRequiredService<IModuleManager>();

        await manager.InitializeAsync(TestContext.Current.CancellationToken);
        await manager.ShutdownAsync(TestContext.Current.CancellationToken);

        LifecycleRecorder.HostBranchLog.ShouldBe([
            "InnerHost.Loaded",
            "RootHost.Loaded",
            "InnerHost.InitializeComposition",
            "RootHost.InitializeRoot",
            "RootHost.ShutdownRoot",
            "InnerHost.ShutdownComposition"
        ]);
    }

    private static class LifecycleRecorder
    {
        [ThreadStatic]
        private static List<string>? _initLog;

        [ThreadStatic]
        private static List<string>? _shutdownLog;

        [ThreadStatic]
        private static List<string>? _hookLog;

        [ThreadStatic]
        private static List<string>? _loadLog;

        [ThreadStatic]
        private static List<string>? _topologyLog;

        [ThreadStatic]
        private static List<string>? _attachmentLog;

        [ThreadStatic]
        private static List<string>? _hostBranchLog;

        public static List<string> InitLog => _initLog ??= [];
        public static List<string> ShutdownLog => _shutdownLog ??= [];
        public static List<string> HookLog => _hookLog ??= [];
        public static List<string> LoadLog => _loadLog ??= [];
        public static List<string> TopologyLog => _topologyLog ??= [];
        public static List<string> AttachmentLog => _attachmentLog ??= [];
        public static List<string> HostBranchLog => _hostBranchLog ??= [];

        public static void Clear()
        {
            _initLog?.Clear();
            _shutdownLog?.Clear();
            _hookLog?.Clear();
            _loadLog?.Clear();
            _topologyLog?.Clear();
            _attachmentLog?.Clear();
            _hostBranchLog?.Clear();
        }
    }

    private sealed class LoadLeafModule : ModuleBase
    {
        public override void OnLoaded(IModuleLoadContext context)
        {
            LifecycleRecorder.LoadLog.Add($"LoadLeafModule.Loaded:{FormatDependents(context.Dependents)}");
        }
    }

    [DependsOn(typeof(LoadLeafModule))]
    private sealed class LoadMiddleModule : ModuleBase
    {
        public override void OnLoaded(IModuleLoadContext context)
        {
            LifecycleRecorder.LoadLog.Add($"LoadMiddleModule.Loaded:{FormatDependents(context.Dependents)}");
        }
    }

    [DependsOn(typeof(LoadMiddleModule))]
    private sealed class LoadRootModule : ModuleBase
    {
        public override void OnLoaded(IModuleLoadContext context)
        {
            LifecycleRecorder.LoadLog.Add($"LoadRootModule.Loaded:{FormatDependents(context.Dependents)}");
        }
    }

    private sealed class FailingLoadModule : ModuleBase
    {
        public override void OnLoaded(IModuleLoadContext context)
        {
            throw new InvalidOperationException("Load failure");
        }
    }

    [DependsOn(typeof(FailingLoadModule))]
    private sealed class FailingLoadRootModule : ModuleBase;

    private sealed class FailingConfigureModule : ModuleBase
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            throw new InvalidOperationException("Configure failure");
        }
    }

    [DependsOn(typeof(FailingConfigureModule))]
    private sealed class FailingConfigureRootModule : ModuleBase;

    private sealed class InvalidApplicationDependencyModule : ApplicationModule;

    [DependsOn(typeof(InvalidApplicationDependencyModule))]
    private sealed class InvalidFrameworkDependsOnApplicationModule : FrameworkCoreModule;

    private sealed class ValidExtensionCapabilityModule : ApplicationModule;

    [DependsOn(typeof(ValidExtensionCapabilityModule))]
    private sealed class InvalidExtensionDependencyModule : ExtensionModule;

    [DependsOn(typeof(InvalidExtensionDependencyModule))]
    private sealed class InvalidApplicationDependsOnExtensionModule : ApplicationModule;

    private sealed class InvalidHostDependencyModule : HostModule;

    [DependsOn(typeof(InvalidHostDependencyModule))]
    private sealed class InvalidExtensionDependsOnHostModule : ExtensionModule;

    private sealed class HookedHostModule : HostModule
    {
        protected override void OnHostLoaded(IModuleLoadContext context)
        {
            LifecycleRecorder.HookLog.Add("HookedHostModule.Loaded");
        }

        protected override Task PreInitializeHostAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.HookLog.Add("HookedHostModule.PreInitializeHook");
            return Task.CompletedTask;
        }

        protected override Task InitializeHostAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.HookLog.Add("HookedHostModule.InitializeHook");
            return Task.CompletedTask;
        }

        protected override Task PostInitializeHostAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.HookLog.Add("HookedHostModule.PostInitializeHook");
            return Task.CompletedTask;
        }

        protected override Task ShutdownHostAsync(IModuleShutdownContext context)
        {
            LifecycleRecorder.HookLog.Add("HookedHostModule.ShutdownHook");
            return Task.CompletedTask;
        }
    }

    private sealed class TopologyAwareSharedFrameworkModule : FrameworkCoreModule
    {
        protected override void OnSharedFoundationLoaded(IModuleLoadContext context)
        {
            LifecycleRecorder.TopologyLog.Add(
                $"Framework:{CurrentModule.Name}:{Category}:{Dependencies.Count}:{Dependents.Count}:{IsLeafFoundation}");
        }
    }

    [DependsOn(typeof(TopologyAwareSharedFrameworkModule))]
    private sealed class TopologyAwareApplicationModule : ApplicationModule
    {
        protected override void OnApplicationCapabilityLoaded(IModuleLoadContext context)
        {
            LifecycleRecorder.TopologyLog.Add(
                $"Application:{CurrentModule.Name}:{HasApplicationDependencies}:{HasExtensionDependents}:{HasHostDependents}");
        }
    }

    [DependsOn(typeof(TopologyAwareApplicationModule))]
    private sealed class TopologyAwareExtensionModule : ExtensionModule
    {
        protected override void OnExtensionAttached(IModuleLoadContext context, IModuleDescriptor attachedCapability)
        {
            LifecycleRecorder.TopologyLog.Add(
                $"Extension:{CurrentModule.Name}:{AttachedCapability.Name}:{AttachedCapabilityCategory}:{IsApplicationExtension}:{IsFrameworkExtension}");
        }
    }

    [DependsOn(typeof(TopologyAwareExtensionModule))]
    private sealed class TopologyAwareInnerHostModule : HostModule
    {
        protected override void OnInnerHostLoaded(IModuleLoadContext context)
        {
            LifecycleRecorder.TopologyLog.Add($"Host:{CurrentModule.Name}:{IsRootHostModule}");
        }
    }

    [DependsOn(typeof(TopologyAwareApplicationModule))]
    [DependsOn(typeof(TopologyAwareExtensionModule))]
    [DependsOn(typeof(TopologyAwareInnerHostModule))]
    private sealed class TopologyAwareRootHostModule : HostModule
    {
        protected override void OnRootHostLoaded(IModuleLoadContext context)
        {
            LifecycleRecorder.TopologyLog.Add($"Host:{CurrentModule.Name}:{IsRootHostModule}");
        }
    }

    private sealed class LeafFoundationModule : FrameworkCoreModule
    {
        protected override void OnLeafFoundationLoaded(IModuleLoadContext context)
        {
            LifecycleRecorder.TopologyLog.Add($"FrameworkLeaf:{CurrentModule.Name}:{IsLeafFoundation}");
        }
    }

    [DependsOn(typeof(LeafFoundationModule))]
    private sealed class LeafFoundationRootHostModule : HostModule;

    private sealed class ApplicationCapabilityModule : ApplicationModule;

    [DependsOn(typeof(ApplicationCapabilityModule))]
    private sealed class ApplicationBoundExtensionModule : ExtensionModule
    {
        protected override void OnExtensionAttached(IModuleLoadContext context, IModuleDescriptor attachedCapability)
        {
            LifecycleRecorder.AttachmentLog.Add(
                $"{CurrentModule.Name}:{AttachedCapability.Name}:{AttachedCapabilityCategory}:{IsApplicationExtension}:{IsFrameworkExtension}");
        }
    }

    [DependsOn(typeof(ApplicationBoundExtensionModule))]
    private sealed class ChainedApplicationExtensionModule : ExtensionModule
    {
        protected override void OnExtensionAttached(IModuleLoadContext context, IModuleDescriptor attachedCapability)
        {
            LifecycleRecorder.AttachmentLog.Add(
                $"{CurrentModule.Name}:{AttachedCapability.Name}:{AttachedCapabilityCategory}:{IsApplicationExtension}:{IsFrameworkExtension}");
        }
    }

    private sealed class FrameworkCapabilityModule : FrameworkCoreModule;

    [DependsOn(typeof(FrameworkCapabilityModule))]
    private sealed class FrameworkBoundExtensionModule : ExtensionModule
    {
        protected override void OnExtensionAttached(IModuleLoadContext context, IModuleDescriptor attachedCapability)
        {
            LifecycleRecorder.AttachmentLog.Add(
                $"{CurrentModule.Name}:{AttachedCapability.Name}:{AttachedCapabilityCategory}:{IsApplicationExtension}:{IsFrameworkExtension}");
        }
    }

    [DependsOn(typeof(ChainedApplicationExtensionModule))]
    [DependsOn(typeof(FrameworkBoundExtensionModule))]
    private sealed class ExtensionAttachmentRootModule : ModuleBase;

    private sealed class FirstApplicationCapabilityModule : ApplicationModule;

    private sealed class SecondApplicationCapabilityModule : ApplicationModule;

    [DependsOn(typeof(FirstApplicationCapabilityModule))]
    [DependsOn(typeof(SecondApplicationCapabilityModule))]
    private sealed class AmbiguousAttachmentExtensionModule : ExtensionModule;

    [DependsOn(typeof(AmbiguousAttachmentExtensionModule))]
    private sealed class AmbiguousExtensionRootModule : ModuleBase;

    private sealed class HostBranchFrameworkModule : FrameworkCoreModule;

    [DependsOn(typeof(HostBranchFrameworkModule))]
    private sealed class BranchingInnerHostModule : HostModule
    {
        protected override void OnInnerHostLoaded(IModuleLoadContext context)
        {
            LifecycleRecorder.HostBranchLog.Add("InnerHost.Loaded");
        }

        protected override Task InitializeHostCompositionAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.HostBranchLog.Add("InnerHost.InitializeComposition");
            return Task.CompletedTask;
        }

        protected override Task ShutdownHostCompositionAsync(IModuleShutdownContext context)
        {
            LifecycleRecorder.HostBranchLog.Add("InnerHost.ShutdownComposition");
            return Task.CompletedTask;
        }
    }

    [DependsOn(typeof(BranchingInnerHostModule))]
    private sealed class BranchingRootHostModule : HostModule
    {
        protected override void OnRootHostLoaded(IModuleLoadContext context)
        {
            LifecycleRecorder.HostBranchLog.Add("RootHost.Loaded");
        }

        protected override Task InitializeRootHostAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.HostBranchLog.Add("RootHost.InitializeRoot");
            return Task.CompletedTask;
        }

        protected override Task ShutdownRootHostAsync(IModuleShutdownContext context)
        {
            LifecycleRecorder.HostBranchLog.Add("RootHost.ShutdownRoot");
            return Task.CompletedTask;
        }
    }

    private sealed class LifecycleLeafModule : ModuleBase
    {
        protected override Task OnPreInitializeAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.InitLog.Add("LifecycleLeafModule.PreInitialize");
            return Task.CompletedTask;
        }

        protected override Task OnInitializeAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.InitLog.Add("LifecycleLeafModule.Initialize");
            return Task.CompletedTask;
        }

        protected override Task OnPostInitializeAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.InitLog.Add("LifecycleLeafModule.PostInitialize");
            return Task.CompletedTask;
        }

        protected override Task OnShutdownAsync(IModuleShutdownContext context)
        {
            LifecycleRecorder.ShutdownLog.Add("LifecycleLeafModule.Shutdown");
            return Task.CompletedTask;
        }
    }

    [DependsOn(typeof(LifecycleLeafModule))]
    private sealed class LifecycleMiddleModule : ModuleBase
    {
        protected override Task OnPreInitializeAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.InitLog.Add("LifecycleMiddleModule.PreInitialize");
            return Task.CompletedTask;
        }

        protected override Task OnInitializeAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.InitLog.Add("LifecycleMiddleModule.Initialize");
            return Task.CompletedTask;
        }

        protected override Task OnPostInitializeAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.InitLog.Add("LifecycleMiddleModule.PostInitialize");
            return Task.CompletedTask;
        }

        protected override Task OnShutdownAsync(IModuleShutdownContext context)
        {
            LifecycleRecorder.ShutdownLog.Add("LifecycleMiddleModule.Shutdown");
            return Task.CompletedTask;
        }
    }

    [DependsOn(typeof(LifecycleMiddleModule))]
    private sealed class LifecycleRootModule : ModuleBase
    {
        protected override Task OnInitializeAsync(IModuleInitializationContext context)
        {
            LifecycleRecorder.InitLog.Add("LifecycleRootModule.Initialize");
            return Task.CompletedTask;
        }

        protected override Task OnShutdownAsync(IModuleShutdownContext context)
        {
            LifecycleRecorder.ShutdownLog.Add("LifecycleRootModule.Shutdown");
            return Task.CompletedTask;
        }
    }

    private sealed class FailingModule : ModuleBase
    {
        protected override Task OnInitializeAsync(IModuleInitializationContext context)
        {
            throw new InvalidOperationException("Intentional failure");
        }
    }

    [DependsOn(typeof(FailingModule))]
    private sealed class FailingRootModule : ModuleBase;

    private static string FormatDependents(IReadOnlyList<IModuleDescriptor> dependents)
    {
        return dependents.Count == 0
            ? "(none)"
            : string.Join(",", dependents.Select(static dependent => dependent.Name));
    }
}
