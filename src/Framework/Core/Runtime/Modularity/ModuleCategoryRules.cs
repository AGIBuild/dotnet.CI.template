namespace ChengYuan.Core.Modularity;

internal static class ModuleCategoryRules
{
    public static void EnsureDependencies(
        IModuleLoadContext context,
        params ModuleCategory[] allowedCategories)
    {
        foreach (var dependency in context.CurrentModule.Dependencies)
        {
            EnsureAllowedCategory(
                context.CurrentModule,
                dependency,
                "depends on",
                allowedCategories);
        }
    }

    public static void EnsureDependents(
        IModuleLoadContext context,
        params ModuleCategory[] allowedCategories)
    {
        foreach (var dependent in context.Dependents)
        {
            EnsureAllowedCategory(
                context.CurrentModule,
                dependent,
                "is depended on by",
                allowedCategories);
        }
    }

    private static void EnsureAllowedCategory(
        IModuleDescriptor currentModule,
        IModuleDescriptor relatedModule,
        string relation,
        IReadOnlyCollection<ModuleCategory> allowedCategories)
    {
        if (allowedCategories.Contains(relatedModule.Category))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Module '{currentModule.ModuleType.FullName}' is categorized as '{currentModule.Category}' and {relation} module '{relatedModule.ModuleType.FullName}' categorized as '{relatedModule.Category}'. Allowed categories: {string.Join(", ", allowedCategories)}.");
    }
}
