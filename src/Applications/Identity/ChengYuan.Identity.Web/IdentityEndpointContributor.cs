using ChengYuan.AspNetCore;
using Microsoft.AspNetCore.Routing;

namespace ChengYuan.Identity;

public sealed class IdentityEndpointContributor : IEndpointContributor
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapIdentityManagementEndpoints();
    }
}
