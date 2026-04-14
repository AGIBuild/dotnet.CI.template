using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ChengYuan.Authorization;

public sealed class PermissionAuthorizationHandler(IPermissionChecker permissionChecker) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (await permissionChecker.IsGrantedAsync(requirement.PermissionName))
        {
            context.Succeed(requirement);
        }
    }
}
