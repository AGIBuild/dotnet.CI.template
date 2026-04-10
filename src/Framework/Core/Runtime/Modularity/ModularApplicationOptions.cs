namespace ChengYuan.Core.Modularity;

public sealed class ModularApplicationOptions
{
    public Type StartupModuleType { get; internal set; } = default!;

    public string? ApplicationName { get; set; }

    public string? EnvironmentName { get; set; }

    public bool EnableDiagnostics { get; set; }
}
