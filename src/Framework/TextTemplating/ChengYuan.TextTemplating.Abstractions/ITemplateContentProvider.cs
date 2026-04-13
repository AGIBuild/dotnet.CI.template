using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.TextTemplating;

public interface ITemplateContentProvider
{
    Task<string?> GetContentOrNullAsync(string templateName, CancellationToken cancellationToken = default);
}
