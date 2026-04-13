using ChengYuan.Core.Lifecycle;

namespace ChengYuan.Core.Modularity;

public abstract class FrameworkCoreModule : ModuleBase
{
    protected bool HasFrameworkDependents => HasDependentCategory(ModuleCategory.FrameworkCore);

    protected bool HasApplicationDependents => HasDependentCategory(ModuleCategory.Application);

    protected bool HasExtensionDependents => HasDependentCategory(ModuleCategory.Extension);

    protected bool HasHostDependents => HasDependentCategory(ModuleCategory.Host);

    protected bool IsLeafFoundation => !HasFrameworkDependents && !HasApplicationDependents && !HasExtensionDependents;

    protected sealed override void ValidateLoad(IModuleLoadContext context)
    {
        ModuleCategoryRules.EnsureDependencies(context, ModuleCategory.FrameworkCore);
        ValidateFrameworkCoreLoad(context);
        ValidateFrameworkComposition(context);
    }

    protected sealed override void OnModuleLoaded(IModuleLoadContext context)
    {
        OnFrameworkCoreLoaded(context);

        if (IsLeafFoundation)
        {
            OnLeafFoundationLoaded(context);
            return;
        }

        OnSharedFoundationLoaded(context);
    }

    protected sealed override Task OnPreInitializeAsync(IModuleInitializationContext context)
    {
        return PreInitializeFrameworkCoreAsync(context);
    }

    protected sealed override Task OnInitializeAsync(IModuleInitializationContext context)
    {
        return InitializeFrameworkCoreAsync(context);
    }

    protected sealed override Task OnPostInitializeAsync(IModuleInitializationContext context)
    {
        return PostInitializeFrameworkCoreAsync(context);
    }

    protected sealed override Task OnShutdownAsync(IModuleShutdownContext context)
    {
        return ShutdownFrameworkCoreAsync(context);
    }

    protected virtual void ValidateFrameworkCoreLoad(IModuleLoadContext context)
    {
    }

    protected virtual void ValidateFrameworkComposition(IModuleLoadContext context)
    {
    }

    protected virtual void OnFrameworkCoreLoaded(IModuleLoadContext context)
    {
    }

    protected virtual void OnSharedFoundationLoaded(IModuleLoadContext context)
    {
    }

    protected virtual void OnLeafFoundationLoaded(IModuleLoadContext context)
    {
    }

    protected virtual Task PreInitializeFrameworkCoreAsync(IModuleInitializationContext context)
    {
        return Task.CompletedTask;
    }

    protected virtual Task InitializeFrameworkCoreAsync(IModuleInitializationContext context)
    {
        return Task.CompletedTask;
    }

    protected virtual Task PostInitializeFrameworkCoreAsync(IModuleInitializationContext context)
    {
        return Task.CompletedTask;
    }

    protected virtual Task ShutdownFrameworkCoreAsync(IModuleShutdownContext context)
    {
        return Task.CompletedTask;
    }
}
