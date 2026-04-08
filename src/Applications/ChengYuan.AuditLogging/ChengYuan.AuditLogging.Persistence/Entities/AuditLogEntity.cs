using System.Text.Json;

namespace ChengYuan.AuditLogging;

public sealed class AuditLogEntity
{
    private string _name = string.Empty;
    private string _serializedProperties = "{}";

    private AuditLogEntity()
    {
    }

    public AuditLogEntity(
        Guid id,
        string name,
        DateTimeOffset startedAtUtc,
        DateTimeOffset? completedAtUtc,
        TimeSpan? duration,
        Guid? tenantId,
        string? userId,
        string? userName,
        bool isAuthenticated,
        string? correlationId,
        bool succeeded,
        string? errorMessage,
        IReadOnlyDictionary<string, object?> properties)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        StartedAtUtc = startedAtUtc;
        CompletedAtUtc = completedAtUtc;
        Duration = duration;
        TenantId = tenantId;
        UserId = userId;
        UserName = userName;
        IsAuthenticated = isAuthenticated;
        CorrelationId = correlationId;
        Succeeded = succeeded;
        ErrorMessage = errorMessage;
        SetName(name);
        SetSerializedProperties(properties);
    }

    public Guid Id { get; private set; }

    public string Name
    {
        get => _name;
        private set => _name = value;
    }

    public DateTimeOffset StartedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public TimeSpan? Duration { get; private set; }

    public Guid? TenantId { get; private set; }

    public string? UserId { get; private set; }

    public string? UserName { get; private set; }

    public bool IsAuthenticated { get; private set; }

    public string? CorrelationId { get; private set; }

    public bool Succeeded { get; private set; }

    public string? ErrorMessage { get; private set; }

    public string SerializedProperties
    {
        get => _serializedProperties;
        private set => _serializedProperties = value;
    }

    public IReadOnlyDictionary<string, object?> ReadProperties()
    {
        using var document = JsonDocument.Parse(SerializedProperties);

        if (document.RootElement.ValueKind is not JsonValueKind.Object)
        {
            return new Dictionary<string, object?>(StringComparer.Ordinal);
        }

        var properties = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var property in document.RootElement.EnumerateObject())
        {
            properties[property.Name] = ConvertJsonValue(property.Value);
        }

        return properties;
    }

    private void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    private void SetSerializedProperties(IReadOnlyDictionary<string, object?> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        SerializedProperties = JsonSerializer.Serialize(properties);
    }

    private static object? ConvertJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.String => element.GetString(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number when element.TryGetInt32(out var intValue) => intValue,
            JsonValueKind.Number when element.TryGetInt64(out var longValue) => longValue,
            JsonValueKind.Number when element.TryGetDecimal(out var decimalValue) => decimalValue,
            JsonValueKind.Number => element.GetDouble(),
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonValue).ToArray(),
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(property => property.Name, property => ConvertJsonValue(property.Value), StringComparer.Ordinal),
            _ => element.GetRawText()
        };
    }
}
