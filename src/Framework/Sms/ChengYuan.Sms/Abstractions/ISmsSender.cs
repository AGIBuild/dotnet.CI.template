using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Sms;

public interface ISmsSender
{
    Task SendAsync(SmsMessage message, CancellationToken cancellationToken = default);
}
