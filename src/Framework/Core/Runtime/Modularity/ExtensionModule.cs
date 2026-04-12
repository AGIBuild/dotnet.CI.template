using ChengYuan.Core.Lifecycle;

namespace ChengYuan.Core.Modularity;

public abstract class ExtensionModule : ModuleBase
{
	private IModuleDescriptor? _attachedCapability;

	protected IModuleDescriptor AttachedCapability => _attachedCapability ?? throw new InvalidOperationException("Attached capability has not been resolved.");

	protected ModuleCategory AttachedCapabilityCategory => AttachedCapability.Category;

	protected bool IsApplicationExtension => AttachedCapabilityCategory == ModuleCategory.Application;

	protected bool IsFrameworkExtension => AttachedCapabilityCategory == ModuleCategory.FrameworkCore;

	protected bool IsPersistenceLikeExtension => CurrentModule.Name.Contains("Persistence", StringComparison.OrdinalIgnoreCase);

	protected sealed override void ValidateLoad(IModuleLoadContext context)
	{
		ModuleCategoryRules.EnsureDependencies(
			context,
			ModuleCategory.FrameworkCore,
			ModuleCategory.Application,
			ModuleCategory.Extension);

		_attachedCapability = ResolveAttachedCapability(context);
		ValidateExtensionAttachment(context, AttachedCapability);
		ValidateExtensionLoad(context);
	}

	protected sealed override void OnModuleLoaded(IModuleLoadContext context)
	{
		OnExtensionLoaded(context);
		OnExtensionAttached(context, AttachedCapability);
	}

	protected sealed override Task OnPreInitializeAsync(IModuleInitializationContext context)
	{
		return PreInitializeExtensionAsync(context);
	}

	protected sealed override Task OnInitializeAsync(IModuleInitializationContext context)
	{
		return InitializeExtensionPipelineAsync(context);
	}

	protected sealed override Task OnPostInitializeAsync(IModuleInitializationContext context)
	{
		return PostInitializeExtensionAsync(context);
	}

	protected sealed override Task OnShutdownAsync(IModuleShutdownContext context)
	{
		return ShutdownExtensionPipelineAsync(context);
	}

	protected virtual void ValidateExtensionLoad(IModuleLoadContext context)
	{
	}

	protected virtual void ValidateExtensionAttachment(IModuleLoadContext context, IModuleDescriptor attachedCapability)
	{
	}

	protected virtual void OnExtensionLoaded(IModuleLoadContext context)
	{
	}

	protected virtual void OnExtensionAttached(IModuleLoadContext context, IModuleDescriptor attachedCapability)
	{
	}

	protected virtual Task PreInitializeExtensionAsync(IModuleInitializationContext context)
	{
		return Task.CompletedTask;
	}

	protected virtual Task InitializeExtensionAsync(IModuleInitializationContext context)
	{
		return Task.CompletedTask;
	}

	protected virtual Task PostInitializeExtensionAsync(IModuleInitializationContext context)
	{
		return Task.CompletedTask;
	}

	protected virtual Task ShutdownExtensionAsync(IModuleShutdownContext context)
	{
		return Task.CompletedTask;
	}

	protected virtual Task InitializeExtensionBindingAsync(IModuleInitializationContext context)
	{
		return Task.CompletedTask;
	}

	protected virtual Task ShutdownExtensionBindingAsync(IModuleShutdownContext context)
	{
		return Task.CompletedTask;
	}

	protected virtual IModuleDescriptor ResolveAttachedCapability(IModuleLoadContext context)
	{
		ArgumentNullException.ThrowIfNull(context);

		var applicationDependencies = GetDependencies(ModuleCategory.Application);
		if (applicationDependencies.Count == 1)
		{
			return applicationDependencies[0];
		}

		if (applicationDependencies.Count > 1)
		{
			throw CreateAmbiguousAttachmentException(applicationDependencies);
		}

		var frameworkDependencies = GetDependencies(ModuleCategory.FrameworkCore);
		if (frameworkDependencies.Count == 1)
		{
			return frameworkDependencies[0];
		}

		if (frameworkDependencies.Count > 1)
		{
			throw CreateAmbiguousAttachmentException(frameworkDependencies);
		}

		var extensionDependencies = GetDependencies(ModuleCategory.Extension);
		if (extensionDependencies.Count == 1
			&& extensionDependencies[0] is ModuleDescriptor extensionDescriptor
			&& extensionDescriptor.Instance is ExtensionModule extensionModule
			&& extensionModule._attachedCapability is not null)
		{
			return extensionModule._attachedCapability;
		}

		if (extensionDependencies.Count > 0)
		{
			throw CreateAmbiguousAttachmentException(extensionDependencies);
		}

		throw new InvalidOperationException(
			$"Extension module '{CurrentModule.ModuleType.FullName}' could not resolve an attached capability from its dependency graph.");
	}

	private async Task InitializeExtensionPipelineAsync(IModuleInitializationContext context)
	{
		await InitializeExtensionAsync(context);
		await InitializeExtensionBindingAsync(context);
	}

	private async Task ShutdownExtensionPipelineAsync(IModuleShutdownContext context)
	{
		await ShutdownExtensionBindingAsync(context);
		await ShutdownExtensionAsync(context);
	}

	private InvalidOperationException CreateAmbiguousAttachmentException(IReadOnlyList<IModuleDescriptor> candidates)
	{
		return new InvalidOperationException(
			$"Extension module '{CurrentModule.ModuleType.FullName}' has ambiguous attached capability candidates: {string.Join(", ", candidates.Select(static candidate => candidate.Name))}.");
	}
}
