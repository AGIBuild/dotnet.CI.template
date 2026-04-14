using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace ChengYuan.TextTemplating;

internal sealed class DefaultTemplateRenderer(
    IOptions<TextTemplatingOptions> options,
    IEnumerable<ITemplateContentProvider> contentProviders) : ITemplateRenderer
{
    private readonly TextTemplatingOptions _options = options.Value;

    public async Task<string> RenderAsync(string templateName, object? model = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateName);

        var content = await ResolveContentAsync(templateName, cancellationToken)
            ?? throw new InvalidOperationException($"Template '{templateName}' was not found.");

        return RenderTemplate(content, model);
    }

    public Task<string> RenderStringAsync(string templateContent, object? model = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateContent);

        return Task.FromResult(RenderTemplate(templateContent, model));
    }

    private async Task<string?> ResolveContentAsync(string templateName, CancellationToken cancellationToken)
    {
        foreach (var provider in contentProviders)
        {
            var content = await provider.GetContentOrNullAsync(templateName, cancellationToken);
            if (content is not null)
            {
                return content;
            }
        }

        if (_options.Templates.TryGetValue(templateName, out var definition) && definition.DefaultContent is not null)
        {
            return definition.DefaultContent;
        }

        return null;
    }

    private static string RenderTemplate(string templateContent, object? model)
    {
        if (model is null)
        {
            return templateContent;
        }

        var result = templateContent;
        var properties = model.GetType().GetProperties();

        foreach (var property in properties)
        {
            var placeholder = $"{{{{{property.Name}}}}}";
            var value = property.GetValue(model)?.ToString() ?? string.Empty;
            result = result.Replace(placeholder, value, StringComparison.Ordinal);
        }

        return result;
    }
}
