namespace ChengYuan.Core.Modularity;

public sealed class ModularApplicationOptions
{
    public Type StartupModuleType { get; internal set; } = default!;

    public IReadOnlyList<Type> AdditionalModuleTypes => _additionalModuleTypes;

    public string? ApplicationName { get; set; }

    public string? EnvironmentName { get; set; }

    public bool EnableDiagnostics { get; set; }

    private readonly List<Type> _additionalModuleTypes = [];

    public void AddAdditionalModule<TModule>()
        where TModule : ModuleBase
    {
        AddAdditionalModule(typeof(TModule));
    }

    public void AddAdditionalModule(Type moduleType)
    {
        ArgumentNullException.ThrowIfNull(moduleType);

        if (!typeof(ModuleBase).IsAssignableFrom(moduleType))
        {
            throw new InvalidOperationException(
                $"Module type '{moduleType.FullName}' must inherit from {nameof(ModuleBase)}.");
        }

        if (_additionalModuleTypes.Contains(moduleType))
        {
            return;
        }

        _additionalModuleTypes.Add(moduleType);
    }
}
