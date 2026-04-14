using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.MultiTenancy;

/// <summary>
/// Internal contract for host-specific tenant input extraction.
/// Each source self-reports availability in the current environment.
/// Not intended for direct use by product developers; use <see cref="MultiTenancyBuilder"/> instead.
/// </summary>
public interface ITenantResolutionSource
{
    /// <summary>
    /// Determines the execution priority. Lower values run first.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Returns true if this source can operate in the current execution environment.
    /// </summary>
    bool IsAvailable(IServiceProvider serviceProvider);

    /// <summary>
    /// Populates the resolve context with candidate tenant values from this source.
    /// </summary>
    ValueTask PopulateAsync(
        TenantResolveContext context,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default);
}
