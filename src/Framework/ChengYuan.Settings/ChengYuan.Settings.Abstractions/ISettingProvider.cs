using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Settings;

public interface ISettingProvider
{
    ValueTask<T?> GetAsync<T>(string name, CancellationToken cancellationToken = default);

    ValueTask<T> GetOrDefaultAsync<T>(string name, T defaultValue, CancellationToken cancellationToken = default);
}
