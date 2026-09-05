using RauchERP.Modules.Identity.Contracts;
using RauchERP.SharedKernel;

namespace RauchERP.Modules.Identity.Application;

public interface IIdentityService
{
    Task<Result<UserResponse>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserResponse>> GetUsersAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<UserResponse?> GetUserAsync(Guid id, CancellationToken cancellationToken);
    Task<Result> SetUserActiveAsync(Guid id, bool active, CancellationToken cancellationToken);
    Task<Result<Guid>> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<RoleResponse>> GetRolesAsync(CancellationToken cancellationToken);
    Task<Result> AssignRolesAsync(Guid userId, IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken);
    Task<Result> AssignPermissionsAsync(Guid roleId, IReadOnlyCollection<Guid> permissionIds, CancellationToken cancellationToken);
    Task<Result<TokenResponse>> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken);
    Task<Result<TokenResponse>> RefreshAsync(RefreshRequest request, string? ipAddress, CancellationToken cancellationToken);
    Task<Result> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken);
}
