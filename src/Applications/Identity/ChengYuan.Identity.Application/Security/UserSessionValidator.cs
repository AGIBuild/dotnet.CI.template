using ChengYuan.Core.DependencyInjection;

namespace ChengYuan.Identity;

[ExposeServices(typeof(IUserSessionValidator))]
public sealed class UserSessionValidator(
    IIdentityUserRepository userRepository) : IUserSessionValidator, IScopedService
{
    public async ValueTask<bool> IsActiveSessionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return false;
        }

        var user = await userRepository.FindDetailsAsync(userId, cancellationToken);
        return user?.IsActive == true;
    }
}
