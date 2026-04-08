using System;

namespace ChengYuan.Authorization;

public sealed record PermissionContext(Guid? TenantId, string? UserId, string? CorrelationId, bool IsAuthenticated);
