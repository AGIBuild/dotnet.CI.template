using System;
using System.Collections.Generic;

namespace ChengYuan.TextTemplating;

public sealed class TextTemplatingOptions
{
    public IDictionary<string, TemplateDefinition> Templates { get; } =
        new Dictionary<string, TemplateDefinition>(StringComparer.Ordinal);

    public void DefineTemplate(string name, string? displayName = null, string? defaultContent = null)
    {
        Templates[name] = new TemplateDefinition(name, displayName, defaultContent);
    }
}
