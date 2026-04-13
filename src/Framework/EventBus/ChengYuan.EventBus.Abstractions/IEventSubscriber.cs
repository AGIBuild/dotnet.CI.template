using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.EventBus;

public interface IEventSubscriber<in TEvent>
    where TEvent : class
{
    ValueTask HandleAsync(TEvent eventData, CancellationToken cancellationToken = default);
}
