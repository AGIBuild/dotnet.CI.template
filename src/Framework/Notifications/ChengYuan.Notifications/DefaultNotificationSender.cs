using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ChengYuan.Notifications;

internal sealed partial class DefaultNotificationSender(
    IEnumerable<INotificationChannel> channels,
    ILogger<DefaultNotificationSender> logger) : INotificationSender
{
    public async Task SendAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        foreach (var channel in channels)
        {
            LogSendingViaChannel(logger, notification.Name, channel.Name);
            await channel.SendAsync(notification, notification.UserIds, cancellationToken);
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Sending notification '{NotificationName}' via channel '{ChannelName}'.")]
    private static partial void LogSendingViaChannel(ILogger logger, string notificationName, string channelName);
}
