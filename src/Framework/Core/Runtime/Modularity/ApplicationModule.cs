using ChengYuan.Core.Lifecycle;

namespace ChengYuan.Core.Modularity;

public abstract class ApplicationModule : ModuleBase
{
	protected bool HasFrameworkDependencies => HasDependencyCategory(ModuleCategory.FrameworkCore);

	protected bool HasApplicationDependencies => HasDependencyCategory(ModuleCategory.Application);

	protected bool HasApplicationDependents => HasDependentCategory(ModuleCategory.Application);

	protected bool HasExtensionDependents => HasDependentCategory(ModuleCategory.Extension);

	protected bool HasHostDependents => HasDependentCategory(ModuleCategory.Host);

	protected sealed override void ValidateLoad(IModuleLoadContext context)
	{
		ModuleCategoryRules.EnsureDependencies(context, ModuleCategory.FrameworkCore, ModuleCategory.Application);
		ValidateApplicationLoad(context);
		ValidateApplicationComposition(context);
	}

	protected sealed override void OnModuleLoaded(IModuleLoadContext context)
	{
		OnApplicationLoaded(context);
		OnApplicationCapabilityLoaded(context);
	}

	protected sealed override Task OnPreInitializeAsync(IModuleInitializationContext context)
	{
		return PreInitializeApplicationAsync(context);
	}

	protected sealed override Task OnInitializeAsync(IModuleInitializationContext context)
	{
		return InitializeApplicationPipelineAsync(context);
	}

	protected sealed override Task OnPostInitializeAsync(IModuleInitializationContext context)
	{
		return PostInitializeApplicationAsync(context);
	}

	protected sealed override Task OnShutdownAsync(IModuleShutdownContext context)
	{
		return ShutdownApplicationPipelineAsync(context);
	}

	protected virtual void ValidateApplicationLoad(IModuleLoadContext context)
	{
	}

	protected virtual void ValidateApplicationComposition(IModuleLoadContext context)
	{
	}

	protected virtual void OnApplicationLoaded(IModuleLoadContext context)
	{
	}

	protected virtual void OnApplicationCapabilityLoaded(IModuleLoadContext context)
	{
	}

	protected virtual Task PreInitializeApplicationAsync(IModuleInitializationContext context)
	{
		return Task.CompletedTask;
	}

	protected virtual Task InitializeApplicationAsync(IModuleInitializationContext context)
	{
		return Task.CompletedTask;
	}

	protected virtual Task PostInitializeApplicationAsync(IModuleInitializationContext context)
	{
		return Task.CompletedTask;
	}

	protected virtual Task ShutdownApplicationAsync(IModuleShutdownContext context)
	{
		return Task.CompletedTask;
	}

	protected virtual Task InitializeHostedApplicationAsync(IModuleInitializationContext context)
	{
		return Task.CompletedTask;
	}

	protected virtual Task ShutdownHostedApplicationAsync(IModuleShutdownContext context)
	{
		return Task.CompletedTask;
	}

	private async Task InitializeApplicationPipelineAsync(IModuleInitializationContext context)
	{
		await InitializeApplicationAsync(context);

		if (HasHostDependents)
		{
			await InitializeHostedApplicationAsync(context);
		}
	}

	private async Task ShutdownApplicationPipelineAsync(IModuleShutdownContext context)
	{
		if (HasHostDependents)
		{
			await ShutdownHostedApplicationAsync(context);
		}

		await ShutdownApplicationAsync(context);
	}
}
