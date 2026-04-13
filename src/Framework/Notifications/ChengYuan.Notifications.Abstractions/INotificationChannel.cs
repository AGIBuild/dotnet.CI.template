using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Notifications;

public interface INotificationChannel
{
    string Name { get; }

    Task SendAsync(Notification notification, IReadOnlyList<string> userIds, CancellationToken cancellationToken = default);
}
