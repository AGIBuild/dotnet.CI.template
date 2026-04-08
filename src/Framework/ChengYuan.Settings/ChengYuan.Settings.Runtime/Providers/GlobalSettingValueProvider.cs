using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Settings;

internal sealed class GlobalSettingValueProvider : ISettingValueProvider
{
    public string Name => "Global";

    public int Order => 100;

    public ValueTask<SettingValue?> GetOrNullAsync(SettingDefinition definition, SettingContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return ValueTask.FromResult<SettingValue?>(null);
    }
}
