using System;
using System.Collections.Generic;

namespace ChengYuan.Notifications;

public sealed class Notification
{
    public required string Name { get; init; }

    public required IReadOnlyList<string> UserIds { get; init; }

    public NotificationSeverity Severity { get; init; } = NotificationSeverity.Info;

    public IDictionary<string, object?> Data { get; init; } = new Dictionary<string, object?>(StringComparer.Ordinal);
}
