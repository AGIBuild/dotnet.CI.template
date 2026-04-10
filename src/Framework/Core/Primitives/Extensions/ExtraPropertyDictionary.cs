namespace ChengYuan.Core.Extensions;

public sealed class ExtraPropertyDictionary : Dictionary<string, object?>
{
    public ExtraPropertyDictionary()
        : base(StringComparer.Ordinal)
    {
    }

    public ExtraPropertyDictionary(IDictionary<string, object?> properties)
        : base(properties, StringComparer.Ordinal)
    {
        ArgumentNullException.ThrowIfNull(properties);
    }
}
