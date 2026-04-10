using System;

namespace ChengYuan.Settings;

public sealed record SettingContext(Guid? TenantId, string? UserId, string? CorrelationId);
