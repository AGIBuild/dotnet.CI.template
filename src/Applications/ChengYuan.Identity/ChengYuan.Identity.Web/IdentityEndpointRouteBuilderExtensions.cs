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
            .WithTags("Identity");

        group.MapGet("/users", static async (IUserReader reader, CancellationToken cancellationToken) =>
            TypedResults.Ok(await reader.GetListAsync(cancellationToken)));

        group.MapGet("/users/{userId:guid}", GetUserByIdAsync);
        group.MapPost("/users", CreateUserAsync);
        group.MapPut("/users/{userId:guid}", UpdateUserAsync);
        group.MapDelete("/users/{userId:guid}", DeleteUserAsync);
        group.MapPut("/users/{userId:guid}/roles/{roleId:guid}", AssignRoleAsync);
        group.MapDelete("/users/{userId:guid}/roles/{roleId:guid}", UnassignRoleAsync);

        group.MapGet("/roles", static async (IRoleReader reader, CancellationToken cancellationToken) =>
            TypedResults.Ok(await reader.GetListAsync(cancellationToken)));

        group.MapGet("/roles/{roleId:guid}", GetRoleByIdAsync);
        group.MapPost("/roles", CreateRoleAsync);
        group.MapPut("/roles/{roleId:guid}", UpdateRoleAsync);
        group.MapDelete("/roles/{roleId:guid}", DeleteRoleAsync);

        return endpoints;
    }

    private static async Task<IResult> GetUserByIdAsync(Guid userId, IUserReader reader, CancellationToken cancellationToken)
    {
        var user = await reader.FindByIdAsync(userId, cancellationToken);
        return user is null ? TypedResults.NotFound() : TypedResults.Ok(user);
    }

    private static async Task<IResult> CreateUserAsync(CreateUserRequest request, IUserManager manager, CancellationToken cancellationToken)
    {
        try
        {
            var user = await manager.CreateAsync(request.UserName, request.Email, cancellationToken);
            return TypedResults.Created($"/identity/users/{user.Id}", user);
        }
        catch (ArgumentException exception)
        {
            return TypedResults.BadRequest(new { error = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return CreateOperationErrorResult(exception);
        }
    }

    private static async Task<IResult> UpdateUserAsync(Guid userId, UpdateUserRequest request, IUserManager manager, CancellationToken cancellationToken)
    {
        try
        {
            var user = await manager.UpdateAsync(userId, request.UserName, request.Email, request.IsActive, cancellationToken);
            return TypedResults.Ok(user);
        }
        catch (ArgumentException exception)
        {
            return TypedResults.BadRequest(new { error = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return CreateOperationErrorResult(exception);
        }
    }

    private static async Task<IResult> DeleteUserAsync(Guid userId, IUserManager manager, CancellationToken cancellationToken)
    {
        try
        {
            await manager.RemoveAsync(userId, cancellationToken);
            return TypedResults.NoContent();
        }
        catch (ArgumentException exception)
        {
            return TypedResults.BadRequest(new { error = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return CreateOperationErrorResult(exception);
        }
    }

    private static async Task<IResult> AssignRoleAsync(Guid userId, Guid roleId, IUserManager manager, CancellationToken cancellationToken)
    {
        try
        {
            var user = await manager.AssignRoleAsync(userId, roleId, cancellationToken);
            return TypedResults.Ok(user);
        }
        catch (ArgumentException exception)
        {
            return TypedResults.BadRequest(new { error = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return CreateOperationErrorResult(exception);
        }
    }

    private static async Task<IResult> UnassignRoleAsync(Guid userId, Guid roleId, IUserManager manager, CancellationToken cancellationToken)
    {
        try
        {
            var user = await manager.UnassignRoleAsync(userId, roleId, cancellationToken);
            return TypedResults.Ok(user);
        }
        catch (ArgumentException exception)
        {
            return TypedResults.BadRequest(new { error = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return CreateOperationErrorResult(exception);
        }
    }

    private static async Task<IResult> GetRoleByIdAsync(Guid roleId, IRoleReader reader, CancellationToken cancellationToken)
    {
        var role = await reader.FindByIdAsync(roleId, cancellationToken);
        return role is null ? TypedResults.NotFound() : TypedResults.Ok(role);
    }

    private static async Task<IResult> CreateRoleAsync(CreateRoleRequest request, IRoleManager manager, CancellationToken cancellationToken)
    {
        try
        {
            var role = await manager.CreateAsync(request.Name, cancellationToken);
            return TypedResults.Created($"/identity/roles/{role.Id}", role);
        }
        catch (ArgumentException exception)
        {
            return TypedResults.BadRequest(new { error = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return CreateOperationErrorResult(exception);
        }
    }

    private static async Task<IResult> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request, IRoleManager manager, CancellationToken cancellationToken)
    {
        try
        {
            var role = await manager.UpdateAsync(roleId, request.Name, request.IsActive, cancellationToken);
            return TypedResults.Ok(role);
        }
        catch (ArgumentException exception)
        {
            return TypedResults.BadRequest(new { error = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return CreateOperationErrorResult(exception);
        }
    }

    private static async Task<IResult> DeleteRoleAsync(Guid roleId, IRoleManager manager, CancellationToken cancellationToken)
    {
        try
        {
            await manager.RemoveAsync(roleId, cancellationToken);
            return TypedResults.NoContent();
        }
        catch (ArgumentException exception)
        {
            return TypedResults.BadRequest(new { error = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return CreateOperationErrorResult(exception);
        }
    }

    private static IResult CreateOperationErrorResult(InvalidOperationException exception)
    {
        return exception.Message.Contains("was not found", StringComparison.Ordinal)
            ? TypedResults.NotFound(new { error = exception.Message })
            : TypedResults.Conflict(new { error = exception.Message });
    }
}
