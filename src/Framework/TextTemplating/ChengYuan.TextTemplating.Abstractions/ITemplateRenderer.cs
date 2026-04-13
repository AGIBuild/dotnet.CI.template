using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.TextTemplating;

public interface ITemplateRenderer
{
    Task<string> RenderAsync(string templateName, object? model = null, CancellationToken cancellationToken = default);

    Task<string> RenderStringAsync(string templateContent, object? model = null, CancellationToken cancellationToken = default);
}
