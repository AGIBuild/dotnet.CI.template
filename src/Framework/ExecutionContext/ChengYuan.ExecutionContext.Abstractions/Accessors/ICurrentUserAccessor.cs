using System;

namespace ChengYuan.ExecutionContext;

public interface ICurrentUserAccessor : ICurrentUser
{
    IDisposable Change(CurrentUserInfo? currentUser);
}
