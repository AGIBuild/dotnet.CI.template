namespace ChengYuan.TextTemplating;

public sealed class TemplateDefinition(string name, string? displayName = null, string? defaultContent = null)
{
    public string Name { get; } = name;

    public string? DisplayName { get; } = displayName;

    public string? DefaultContent { get; } = defaultContent;
}
