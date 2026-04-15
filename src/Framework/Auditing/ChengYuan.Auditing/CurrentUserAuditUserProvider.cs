using ChengYuan.Core.Data.Auditing;
using ChengYuan.ExecutionContext;

namespace ChengYuan.Auditing;

internal sealed class CurrentUserAuditUserProvider(ICurrentUser currentUser) : IAuditUserProvider
{
    public string? UserId => currentUser.Id;
}
