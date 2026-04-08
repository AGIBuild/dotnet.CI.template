using System;

namespace ChengYuan.Features;

public sealed record FeatureContext(Guid? TenantId, string? UserId, string? CorrelationId);
