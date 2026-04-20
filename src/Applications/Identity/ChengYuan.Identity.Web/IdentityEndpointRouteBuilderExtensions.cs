using ChengYuan.Core.Application.Dtos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ChengYuan.Identity;

public static class IdentityEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapIdentityManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/identity")
            .WithTags("Identity")
            .RequireAuthorization();

        group.MapPost("/login", LoginAsync).AllowAnonymous();

        group.MapGet("/users", static async (IUserReader reader, CancellationToken cancellationToken) =>
            TypedResults.Ok(new ListResultDto<UserRecord>(await reader.GetListAsync(cancellationToken))))
            .RequireAuthorization(IdentityPermissions.Users);

        group.MapGet("/users/{userId:guid}", GetUserByIdAsync).RequireAuthorization(IdentityPermissions.Users);
        group.MapPost("/users", CreateUserAsync).RequireAuthorization(IdentityPermissions.UsersCreate);
        group.MapPut("/users/{userId:guid}", UpdateUserAsync).RequireAuthorization(IdentityPermissions.UsersUpdate);
        group.MapDelete("/users/{userId:guid}", DeleteUserAsync).RequireAuthorization(IdentityPermissions.UsersDelete);
        group.MapPut("/users/{userId:guid}/roles/{roleId:guid}", AssignRoleAsync).RequireAuthorization(IdentityPermissions.UsersUpdate);
        group.MapDelete("/users/{userId:guid}/roles/{roleId:guid}", UnassignRoleAsync).RequireAuthorization(IdentityPermissions.UsersUpdate);

        group.MapGet("/roles", static async (IRoleReader reader, CancellationToken cancellationToken) =>
            TypedResults.Ok(new ListResultDto<RoleRecord>(await reader.GetListAsync(cancellationToken))))
            .RequireAuthorization(IdentityPermissions.Roles);

        group.MapGet("/roles/{roleId:guid}", GetRoleByIdAsync).RequireAuthorization(IdentityPermissions.Roles);
        group.MapPost("/roles", CreateRoleAsync).RequireAuthorization(IdentityPermissions.RolesCreate);
        group.MapPut("/roles/{roleId:guid}", UpdateRoleAsync).RequireAuthorization(IdentityPermissions.RolesUpdate);
        group.MapDelete("/roles/{roleId:guid}", DeleteRoleAsync).RequireAuthorization(IdentityPermissions.RolesDelete);

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IUserManager manager,
        JwtTokenService tokenService,
        CancellationToken cancellationToken)
    {
        var user = await manager.VerifyPasswordAsync(request.UserName, request.Password, cancellationToken);
        if (user is null)
        {
            return TypedResults.Unauthorized();
        }

        var token = tokenService.GenerateToken(user);
        return TypedResults.Ok(token);
    }

    private static async Task<IResult> GetUserByIdAsync(Guid userId, IUserReader reader, CancellationToken cancellationToken)
    {
        var user = await reader.FindByIdAsync(userId, cancellationToken);
        return user is null ? TypedResults.NotFound() : TypedResults.Ok(user);
    }

    private static async Task<IResult> CreateUserAsync(CreateUserRequest request, IUserManager manager, CancellationToken cancellationToken)
    {
        var user = await manager.CreateAsync(request.UserName, request.Email, request.Password, cancellationToken);
        return TypedResults.Created($"/identity/users/{user.Id}", user);
    }

    private static async Task<IResult> UpdateUserAsync(Guid userId, UpdateUserRequest request, IUserManager manager, CancellationToken cancellationToken)
    {
        var user = await manager.UpdateAsync(userId, request.UserName, request.Email, request.IsActive, cancellationToken);
        return TypedResults.Ok(user);
    }

    private static async Task<IResult> DeleteUserAsync(Guid userId, IUserManager manager, CancellationToken cancellationToken)
    {
        await manager.RemoveAsync(userId, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> AssignRoleAsync(Guid userId, Guid roleId, IUserManager manager, CancellationToken cancellationToken)
    {
        var user = await manager.AssignRoleAsync(userId, roleId, cancellationToken);
        return TypedResults.Ok(user);
    }

    private static async Task<IResult> UnassignRoleAsync(Guid userId, Guid roleId, IUserManager manager, CancellationToken cancellationToken)
    {
        var user = await manager.UnassignRoleAsync(userId, roleId, cancellationToken);
        return TypedResults.Ok(user);
    }

    private static async Task<IResult> GetRoleByIdAsync(Guid roleId, IRoleReader reader, CancellationToken cancellationToken)
    {
        var role = await reader.FindByIdAsync(roleId, cancellationToken);
        return role is null ? TypedResults.NotFound() : TypedResults.Ok(role);
    }

    private static async Task<IResult> CreateRoleAsync(CreateRoleRequest request, IRoleManager manager, CancellationToken cancellationToken)
    {
        var role = await manager.CreateAsync(request.Name, cancellationToken);
        return TypedResults.Created($"/identity/roles/{role.Id}", role);
    }

    private static async Task<IResult> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request, IRoleManager manager, CancellationToken cancellationToken)
    {
        var role = await manager.UpdateAsync(roleId, request.Name, request.IsActive, cancellationToken);
        return TypedResults.Ok(role);
    }

    private static async Task<IResult> DeleteRoleAsync(Guid roleId, IRoleManager manager, CancellationToken cancellationToken)
    {
        await manager.RemoveAsync(roleId, cancellationToken);
        return TypedResults.NoContent();
    }
}
