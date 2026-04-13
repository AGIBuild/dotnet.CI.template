namespace ChengYuan.Core;

public static class ScopeValidator
{
    private const int Global = 0;
    private const int Tenant = 1;
    private const int User = 2;

    public static void ValidateScopeArguments<TScope>(TScope scope, Guid? tenantId, string? userId, string resourceLabel)
        where TScope : struct, Enum
    {
        var scopeValue = Convert.ToInt32(scope, System.Globalization.CultureInfo.InvariantCulture);

        switch (scopeValue)
        {
            case Global:
                if (tenantId is not null || !string.IsNullOrWhiteSpace(userId))
                {
                    throw new ArgumentException($"Global {resourceLabel} cannot specify tenant or user values.");
                }

                break;

            case Tenant:
                if (tenantId is null || tenantId == Guid.Empty)
                {
                    throw new ArgumentException($"Tenant {resourceLabel} must specify a non-empty tenant id.", nameof(tenantId));
                }

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    throw new ArgumentException($"Tenant {resourceLabel} cannot specify a user id.", nameof(userId));
                }

                break;

            case User:
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new ArgumentException($"User {resourceLabel} must specify a user id.", nameof(userId));
                }

                if (tenantId is not null)
                {
                    throw new ArgumentException($"User {resourceLabel} are keyed by user id only and cannot specify a tenant id.", nameof(tenantId));
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(scope), scope, $"Unsupported {resourceLabel} scope.");
        }
    }
}
