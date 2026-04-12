using ChengYuan.Core.Lifecycle;

namespace ChengYuan.Core.Modularity;

public abstract class HostModule : ModuleBase
{
	protected bool IsRootHostModule => IsRoot;

	protected bool HasHostDependencies => HasDependencyCategory(ModuleCategory.Host);

	protected bool HasHostDependents => HasDependentCategory(ModuleCategory.Host);

	protected sealed override void ValidateLoad(IModuleLoadContext context)
	{
		ModuleCategoryRules.EnsureDependents(context, ModuleCategory.Host);
		ValidateHostLoad(context);
		ValidateHostComposition(context);
	}

	protected sealed override void OnModuleLoaded(IModuleLoadContext context)
	{
		OnHostLoaded(context);

		if (IsRootHostModule)
		{
			OnRootHostLoaded(context);
			return;
		}

		OnInnerHostLoaded(context);
	}

	protected sealed override Task OnPreInitializeAsync(IModuleInitializationContext context)
	{
		return PreInitializeHostAsync(context);
	}

	protected sealed override Task OnInitializeAsync(IModuleInitializationContext context)
	{
		return InitializeHostPipelineAsync(context);
	}

	protected sealed override Task OnPostInitializeAsync(IModuleInitializationContext context)
	{
		return PostInitializeHostAsync(context);
	}

	protected sealed override Task OnShutdownAsync(IModuleShutdownContext context)
	{
		return ShutdownHostPipelineAsync(context);
	}

	protected virtual void ValidateHostLoad(IModuleLoadContext context)
	{
	}

	protected virtual void ValidateHostComposition(IModuleLoadContext context)
	{
	}

	protected virtual void OnHostLoaded(IModuleLoadContext context)
	{
	}

	protected virtual void OnRootHostLoaded(IModuleLoadContext context)
	{
	}

	protected virtual void OnInnerHostLoaded(IModuleLoadContext context)
	{
	}

	protected virtual Task PreInitializeHostAsync(IModuleInitializationContext context)
	{
		return Task.CompletedTask;
	}

	protected virtual Task InitializeHostAsync(IModuleInitializationContext context)
	{
		return Task.CompletedTask;
	}

	protected virtual Task PostInitializeHostAsync(IModuleInitializationContext context)
	{
		return Task.CompletedTask;
	}

	protected virtual Task ShutdownHostAsync(IModuleShutdownContext context)
	{
		return Task.CompletedTask;
	}

	protected virtual Task InitializeRootHostAsync(IModuleInitializationContext context)
	{
		return Task.CompletedTask;
	}

	protected virtual Task InitializeHostCompositionAsync(IModuleInitializationContext context)
	{
		return Task.CompletedTask;
	}

	protected virtual Task ShutdownRootHostAsync(IModuleShutdownContext context)
	{
		return Task.CompletedTask;
	}

	protected virtual Task ShutdownHostCompositionAsync(IModuleShutdownContext context)
	{
		return Task.CompletedTask;
	}

	private async Task InitializeHostPipelineAsync(IModuleInitializationContext context)
	{
		await InitializeHostAsync(context);

		if (IsRootHostModule)
		{
			await InitializeRootHostAsync(context);
			return;
		}

		await InitializeHostCompositionAsync(context);
	}

	private async Task ShutdownHostPipelineAsync(IModuleShutdownContext context)
	{
		if (IsRootHostModule)
		{
			await ShutdownRootHostAsync(context);
		}
		else
		{
			await ShutdownHostCompositionAsync(context);
		}

		await ShutdownHostAsync(context);
	}
}
