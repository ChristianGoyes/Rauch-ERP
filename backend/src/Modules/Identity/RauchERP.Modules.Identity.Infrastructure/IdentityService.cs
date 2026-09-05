using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RauchERP.Modules.Identity.Application;
using RauchERP.Modules.Identity.Contracts;
using RauchERP.Modules.Identity.Domain;
using RauchERP.SharedKernel;

namespace RauchERP.Modules.Identity.Infrastructure;

internal sealed class IdentityService(IdentityDbContext db, IConfiguration configuration) : IIdentityService
{
    private DateTimeOffset Now => DateTimeOffset.UtcNow;
    public async Task<Result<UserResponse>> CreateUserAsync(CreateUserRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email, ct)) return Result<UserResponse>.Failure(new("identity.email_conflict", "The email is already registered."));
        if (request.Password.Length < 10) return Result<UserResponse>.Failure(new("identity.weak_password", "Password must contain at least 10 characters."));
        if (!SystemRoles.Assignable.Contains(request.Role)) return Result<UserResponse>.Failure(new("identity.role_invalid", "Select a valid system role."));
        var role = await db.Roles.SingleOrDefaultAsync(x => x.Name == request.Role, ct);
        if (role is null) return Result<UserResponse>.Failure(new("identity.role_not_found", "The selected role is not configured."));
        var user = User.Create(email, request.DisplayName, BCrypt.Net.BCrypt.HashPassword(request.Password, 12), Now);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.Users.Add(user); await db.SaveChangesAsync(ct);
        db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return Result<UserResponse>.Success(Map(user, [role.Name]));
    }
    public async Task<IReadOnlyList<UserResponse>> GetUsersAsync(int page, int pageSize, CancellationToken ct)
    {
        var users = await db.Users.AsNoTracking().OrderBy(x => x.Email).Skip((Math.Max(1, page) - 1) * Math.Clamp(pageSize, 1, 100)).Take(Math.Clamp(pageSize, 1, 100)).ToListAsync(ct);
        var ids = users.Select(x => x.Id).ToList();
        var memberships = await (from ur in db.UserRoles.AsNoTracking() join role in db.Roles.AsNoTracking() on ur.RoleId equals role.Id where ids.Contains(ur.UserId) select new { ur.UserId, role.Name }).ToListAsync(ct);
        return users.Select(x => Map(x, memberships.Where(m => m.UserId == x.Id).Select(m => m.Name).ToList())).ToList();
    }
    public async Task<UserResponse?> GetUserAsync(Guid id, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct); if (user is null) return null;
        var roles = await (from ur in db.UserRoles.AsNoTracking() join role in db.Roles.AsNoTracking() on ur.RoleId equals role.Id where ur.UserId == id select role.Name).ToListAsync(ct);
        return Map(user, roles);
    }
    public async Task<Result> SetUserActiveAsync(Guid id, bool active, CancellationToken ct)
    { var user = await db.Users.FindAsync([id], ct); if (user is null) return Result.Failure(new("identity.user_not_found", "User was not found.")); user.SetActive(active, Now); await db.SaveChangesAsync(ct); return Result.Success(); }
    public async Task<Result<Guid>> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct)
    { var name = request.Name.Trim(); if (string.IsNullOrWhiteSpace(name)) return Result<Guid>.Failure(new("identity.role_name_required", "Role name is required.")); if (await db.Roles.AnyAsync(x => x.Name == name, ct)) return Result<Guid>.Failure(new("identity.role_conflict", "Role already exists.")); var role = new Role(name); db.Roles.Add(role); await db.SaveChangesAsync(ct); return Result<Guid>.Success(role.Id); }
    public async Task<IReadOnlyList<RoleResponse>> GetRolesAsync(CancellationToken ct)
    {
        var roles = await db.Roles.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);
        var permissions = await (from rp in db.RolePermissions.AsNoTracking() join p in db.Permissions.AsNoTracking() on rp.PermissionId equals p.Id select new { rp.RoleId, p.Code }).ToListAsync(ct);
        return roles.Select(x => new RoleResponse(x.Id, x.Name, permissions.Where(p => p.RoleId == x.Id).Select(p => p.Code).ToList())).ToList();
    }
    public async Task<Result> AssignRolesAsync(Guid userId, IReadOnlyCollection<Guid> roleIds, CancellationToken ct)
    {
        if (!await db.Users.AnyAsync(x => x.Id == userId, ct)) return Result.Failure(new("identity.user_not_found", "User was not found."));
        var valid = await db.Roles.Where(x => roleIds.Contains(x.Id)).Select(x => x.Id).ToListAsync(ct);
        if (valid.Count != roleIds.Distinct().Count()) return Result.Failure(new("identity.role_not_found", "One or more roles were not found."));
        var old = await db.UserRoles.Where(x => x.UserId == userId).ToListAsync(ct); db.UserRoles.RemoveRange(old); db.UserRoles.AddRange(valid.Select(x => new UserRole { UserId = userId, RoleId = x })); await db.SaveChangesAsync(ct); return Result.Success();
    }
    public async Task<Result> AssignPermissionsAsync(Guid roleId, IReadOnlyCollection<Guid> permissionIds, CancellationToken ct)
    {
        if (!await db.Roles.AnyAsync(x => x.Id == roleId, ct)) return Result.Failure(new("identity.role_not_found", "Role was not found."));
        var valid = await db.Permissions.Where(x => permissionIds.Contains(x.Id)).Select(x => x.Id).ToListAsync(ct);
        if (valid.Count != permissionIds.Distinct().Count()) return Result.Failure(new("identity.permission_not_found", "One or more permissions were not found."));
        var old = await db.RolePermissions.Where(x => x.RoleId == roleId).ToListAsync(ct); db.RolePermissions.RemoveRange(old); db.RolePermissions.AddRange(valid.Select(x => new RolePermission { RoleId = roleId, PermissionId = x })); await db.SaveChangesAsync(ct); return Result.Success();
    }
    public async Task<Result<TokenResponse>> LoginAsync(LoginRequest request, string? ip, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant(); var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email, ct);
        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return Result<TokenResponse>.Failure(new("identity.invalid_credentials", "Invalid credentials."));
        return Result<TokenResponse>.Success(await IssueAsync(user, ip, ct));
    }
    public async Task<Result<TokenResponse>> RefreshAsync(RefreshRequest request, string? ip, CancellationToken ct)
    {
        var hash = Hash(request.RefreshToken); var session = await db.Sessions.SingleOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (session is null || !session.IsValid(Now)) return Result<TokenResponse>.Failure(new("identity.invalid_refresh_token", "Refresh token is invalid or revoked."));
        var user = await db.Users.FindAsync([session.UserId], ct); if (user is null || !user.IsActive) return Result<TokenResponse>.Failure(new("identity.user_inactive", "User is inactive."));
        await using var tx = await db.Database.BeginTransactionAsync(ct); var response = await IssueAsync(user, ip, ct); var replacement = await db.Sessions.SingleAsync(x => x.TokenHash == Hash(response.RefreshToken), ct); session.Revoke(Now, replacement.Id); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return Result<TokenResponse>.Success(response);
    }
    public async Task<Result> LogoutAsync(LogoutRequest request, CancellationToken ct)
    { var session = await db.Sessions.SingleOrDefaultAsync(x => x.TokenHash == Hash(request.RefreshToken), ct); if (session is null || !session.IsValid(Now)) return Result.Failure(new("identity.invalid_refresh_token", "Refresh token is invalid or revoked.")); session.Revoke(Now); await db.SaveChangesAsync(ct); return Result.Success(); }
    private async Task<TokenResponse> IssueAsync(User user, string? ip, CancellationToken ct)
    {
        var permissions = await (from ur in db.UserRoles join rp in db.RolePermissions on ur.RoleId equals rp.RoleId join p in db.Permissions on rp.PermissionId equals p.Id where ur.UserId == user.Id select p.Code).Distinct().ToListAsync(ct);
        var minutes = configuration.GetValue("Jwt:AccessTokenMinutes", 15); var expires = Now.AddMinutes(minutes); var key = configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is required.");
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.Id.ToString()), new(ClaimTypes.Email, user.Email), new(ClaimTypes.Name, user.DisplayName) }; claims.AddRange(permissions.Select(x => new Claim("permission", x)));
        var jwt = new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"], claims, expires: expires.UtcDateTime, signingCredentials: new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
        var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)); var session = new UserSession(user.Id, Hash(refresh), Now, Now.AddDays(configuration.GetValue("Jwt:RefreshTokenDays", 7)), ip); db.Sessions.Add(session); await db.SaveChangesAsync(ct);
        return new(new JwtSecurityTokenHandler().WriteToken(jwt), refresh, expires);
    }
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static UserResponse Map(User x, IReadOnlyCollection<string> roles) => new(x.Id, x.Email, x.DisplayName, x.IsActive, x.CreatedAt, roles);
}
