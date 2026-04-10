using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Settings;

public interface ISettingValueProvider
{
    string Name { get; }

    int Order { get; }

    ValueTask<SettingValue?> GetOrNullAsync(SettingDefinition definition, SettingContext context, CancellationToken cancellationToken = default);
}
