using System;
using System.Threading;
using ChengYuan.Core;

namespace ChengYuan.ExecutionContext;

internal sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    private static readonly CurrentUserInfo AnonymousUser = new(null, null, false);
    private readonly AsyncLocal<CurrentUserInfo?> _currentUser = new();

    public string? Id => (_currentUser.Value ?? AnonymousUser).Id;

    public string? UserName => (_currentUser.Value ?? AnonymousUser).UserName;

    public bool IsAuthenticated => (_currentUser.Value ?? AnonymousUser).IsAuthenticated;

    public IDisposable Change(CurrentUserInfo? currentUser)
    {
        var previousUser = _currentUser.Value;
        _currentUser.Value = currentUser;
        return new DelegateDisposable(() => _currentUser.Value = previousUser);
    }
}
