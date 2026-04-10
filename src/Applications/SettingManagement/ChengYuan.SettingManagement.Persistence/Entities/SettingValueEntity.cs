using System.Text.Json;
using ChengYuan.Settings;

namespace ChengYuan.SettingManagement;

public sealed class SettingValueEntity
{
    private string _name = string.Empty;
    private string _serializedValue = "null";

    private SettingValueEntity()
    {
    }

    public SettingValueEntity(Guid id, string name, SettingScope scope, object? value, Guid? tenantId = null, string? userId = null)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Scope = scope;
        TenantId = tenantId;
        UserId = userId;
        SetName(name);
        SetSerializedValue(value);
    }

    public Guid Id { get; private set; }

    public string Name
    {
        get => _name;
        private set => _name = value;
    }

    public SettingScope Scope { get; private set; }

    public string SerializedValue
    {
        get => _serializedValue;
        private set => _serializedValue = value;
    }

    public Guid? TenantId { get; private set; }

    public string? UserId { get; private set; }

    public void Update(string name, object? value)
    {
        SetName(name);
        SetSerializedValue(value);
    }

    public object? ReadValue()
    {
        using var document = JsonDocument.Parse(SerializedValue);
        return document.RootElement.Clone();
    }

    private void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    private void SetSerializedValue(object? value)
    {
        SerializedValue = value switch
        {
            JsonElement jsonElement => jsonElement.GetRawText(),
            null => "null",
            _ => JsonSerializer.Serialize(value, value.GetType())
        };
    }
}
