using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Settings;

internal sealed class InMemoryTenantSettingValueProvider(IReadOnlyDictionary<(Guid TenantId, string Name), object?> values) : ISettingValueProvider
{
    public string Name => "InMemoryTenant";

    public int Order => 250;

    public ValueTask<SettingValue?> GetOrNullAsync(SettingDefinition definition, SettingContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (context.TenantId is not Guid tenantId)
        {
            return ValueTask.FromResult<SettingValue?>(null);
        }

        return ValueTask.FromResult(
            values.TryGetValue((tenantId, definition.Name), out var value)
                ? new SettingValue(value, Name)
                : null);
    }
}
