using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ChengYuan.Notifications;

internal sealed partial class NullNotificationChannel(ILogger<NullNotificationChannel> logger) : INotificationChannel
{
    public string Name => "Null";

    public Task SendAsync(Notification notification, IReadOnlyList<string> userIds, CancellationToken cancellationToken = default)
    {
        LogNotificationSkipped(logger, notification.Name, userIds.Count);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Notification '{NotificationName}' for {UserCount} user(s) was not sent (NullNotificationChannel is active).")]
    private static partial void LogNotificationSkipped(ILogger logger, string notificationName, int userCount);
}
