namespace RauchERP.Modules.Identity.Contracts;

public static class Permissions
{
    public static readonly string[] All =
    [
        "identity.users.read", "identity.users.create", "identity.users.update",
        "identity.roles.read", "identity.roles.create", "identity.roles.update",
        "organization.companies.read", "organization.companies.create",
        "organization.branches.read", "organization.branches.create",
        "catalog.categories.read", "catalog.categories.create",
        "catalog.products.read", "catalog.products.create", "catalog.products.update",
        "inventory.stock.read", "inventory.movements.read", "inventory.adjustments.create",
        "inventory.purchases.read", "inventory.purchases.create",
        "sales.read", "sales.create", "dashboard.read"
    ];
}

public static class SystemRoles
{
    public const string Administrator = "Administrator";
    public const string Sales = "Sales";
    public const string Accountant = "Accountant";
    public static readonly string[] Assignable = [Administrator, Sales, Accountant];
}

public sealed record CreateUserRequest(string Email, string DisplayName, string Password, string Role);
public sealed record UserResponse(Guid Id, string Email, string DisplayName, bool IsActive, DateTimeOffset CreatedAt, IReadOnlyCollection<string> Roles);
public sealed record RoleResponse(Guid Id, string Name, IReadOnlyCollection<string> Permissions);
public sealed record CreateRoleRequest(string Name);
public sealed record AssignIdsRequest(IReadOnlyCollection<Guid> Ids);
public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);
public sealed record TokenResponse(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);
