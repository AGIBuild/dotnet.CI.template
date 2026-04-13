using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.BackgroundJobs;

public interface IBackgroundJob<in TArgs>
{
    Task ExecuteAsync(TArgs args, CancellationToken cancellationToken = default);
}
