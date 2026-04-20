using Microsoft.AspNetCore.Routing;

namespace ChengYuan.AspNetCore;

public interface IEndpointContributor
{
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
