using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Notifications;

public interface INotificationSender
{
    Task SendAsync(Notification notification, CancellationToken cancellationToken = default);
}
