namespace ChengYuan.Core.Extensions;

public sealed class ExtraPropertyDefinition
{
    private readonly Dictionary<string, object?> _metadata = new(StringComparer.Ordinal);

    internal ExtraPropertyDefinition(string name, Type propertyType)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
        PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
    }

    public string Name { get; }

    public Type PropertyType { get; }

    public object? DefaultValue { get; internal set; }

    public string? DisplayName { get; internal set; }

    public bool IsRequired { get; internal set; }

    public IReadOnlyDictionary<string, object?> Metadata => _metadata;

    public bool TryGetMetadata(string name, out object? value)
    {
        var metadataName = Check.NotNullOrWhiteSpace(name, nameof(name));
        return _metadata.TryGetValue(metadataName, out value);
    }

    internal void SetMetadata(string name, object? value)
    {
        var metadataName = Check.NotNullOrWhiteSpace(name, nameof(name));
        _metadata[metadataName] = value;
    }
}
