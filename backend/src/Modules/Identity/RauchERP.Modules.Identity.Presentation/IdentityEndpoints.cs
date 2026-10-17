using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RauchERP.Modules.Identity.Application;
using RauchERP.Modules.Identity.Contracts;
using RauchERP.SharedKernel;

namespace RauchERP.Modules.Identity.Presentation;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/v1/auth").WithTags("Auth");
        auth.MapPost("/login", async (LoginRequest r, HttpContext h, IIdentityService s, CancellationToken ct) => ToHttp(await s.LoginAsync(r, h.Connection.RemoteIpAddress?.ToString(), ct)));
        auth.MapPost("/refresh", async (RefreshRequest r, HttpContext h, IIdentityService s, CancellationToken ct) => ToHttp(await s.RefreshAsync(r, h.Connection.RemoteIpAddress?.ToString(), ct)));
        auth.MapPost("/logout", async (LogoutRequest r, IIdentityService s, CancellationToken ct) => ToHttp(await s.LogoutAsync(r, ct))).RequireAuthorization();
        var users = app.MapGroup("/api/v1/users").WithTags("Users").RequireAuthorization();
        users.MapPost("/", async (CreateUserRequest r, IIdentityService s, CancellationToken ct) => ToHttp(await s.CreateUserAsync(r, ct))).RequireAuthorization("identity.users.create");
        users.MapGet("/", async (int? page, int? pageSize, IIdentityService s, CancellationToken ct) => Results.Ok(await s.GetUsersAsync(page ?? 1, pageSize ?? 50, ct))).RequireAuthorization("identity.users.read");
        users.MapGet("/{id:guid}", async (Guid id, IIdentityService s, CancellationToken ct) => await s.GetUserAsync(id, ct) is { } user ? Results.Ok(user) : Results.NotFound()).RequireAuthorization("identity.users.read");
        users.MapPut("/{id:guid}/active", async (Guid id, bool active, IIdentityService s, CancellationToken ct) => ToHttp(await s.SetUserActiveAsync(id, active, ct))).RequireAuthorization("identity.users.update");
        users.MapPut("/{id:guid}/roles", async (Guid id, AssignIdsRequest r, IIdentityService s, CancellationToken ct) => ToHttp(await s.AssignRolesAsync(id, r.Ids, ct))).RequireAuthorization("identity.users.update");
        var roles = app.MapGroup("/api/v1/roles").WithTags("Roles").RequireAuthorization();
        roles.MapGet("/", async (IIdentityService s, CancellationToken ct) => Results.Ok(await s.GetRolesAsync(ct))).RequireAuthorization("identity.roles.read");
        roles.MapPost("/", async (CreateRoleRequest r, IIdentityService s, CancellationToken ct) => ToHttp(await s.CreateRoleAsync(r, ct))).RequireAuthorization("identity.roles.create");
        roles.MapPut("/{id:guid}/permissions", async (Guid id, AssignIdsRequest r, IIdentityService s, CancellationToken ct) => ToHttp(await s.AssignPermissionsAsync(id, r.Ids, ct))).RequireAuthorization("identity.roles.update");
        return app;
    }
    private static IResult ToHttp(Result result) => result.IsSuccess ? Results.NoContent() : Results.Problem(result.Error.Description, statusCode: 400, title: result.Error.Code);
    private static IResult ToHttp<T>(Result<T> result) => result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Description, statusCode: 400, title: result.Error.Code);
}
