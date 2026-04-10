using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Features;

public interface IFeatureChecker
{
    ValueTask<T?> GetAsync<T>(string name, CancellationToken cancellationToken = default);

    ValueTask<T> GetOrDefaultAsync<T>(string name, T defaultValue, CancellationToken cancellationToken = default);

    ValueTask<bool> IsEnabledAsync(string name, CancellationToken cancellationToken = default);
}
