namespace ChengYuan.Core.Data;

/// <summary>
/// Specifies the named connection string that a DbContext should use.
/// When not applied, the "Default" connection string is used.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ConnectionStringNameAttribute(string name) : Attribute
{
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    public static string GetNameOrDefault(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type.GetCustomAttributes(typeof(ConnectionStringNameAttribute), true) is
            [ConnectionStringNameAttribute attr, ..] ? attr.Name : ConnectionStringOptions.DefaultConnectionStringName;
    }
}
