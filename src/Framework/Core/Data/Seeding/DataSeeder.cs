using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ChengYuan.Core.Data;

public sealed partial class DataSeeder(
    IEnumerable<IDataSeedContributor> contributors,
    ILogger<DataSeeder> logger) : IDataSeeder
{
    public async ValueTask SeedAsync(DataSeedContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var contributor in contributors)
        {
            LogExecutingContributor(logger, contributor.GetType().FullName);
            await contributor.SeedAsync(context, cancellationToken);
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Executing data seed contributor: {ContributorType}")]
    private static partial void LogExecutingContributor(ILogger logger, string? contributorType);
}
