using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.Core.Data;

public interface IDataSeeder
{
    ValueTask SeedAsync(DataSeedContext context, CancellationToken cancellationToken = default);
}
