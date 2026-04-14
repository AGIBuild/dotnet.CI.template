using System;

namespace ChengYuan.MultiTenancy;

/// <summary>
/// Normalized tenant lookup result returned by <see cref="ITenantResolutionStore"/>.
/// Carries just enough information for the resolution pipeline to decide the outcome.
/// </summary>
public sealed record TenantResolutionRecord(Guid Id, string Name, bool IsActive);
