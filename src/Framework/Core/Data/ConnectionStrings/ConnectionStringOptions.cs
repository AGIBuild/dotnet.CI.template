using System.Collections.Generic;

namespace ChengYuan.Core.Data;

public sealed class ConnectionStringOptions
{
    public const string DefaultConnectionStringName = "Default";

    public string? Default { get; set; }

    public Dictionary<string, string> ConnectionStrings { get; } = new(System.StringComparer.Ordinal);
}
