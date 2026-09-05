using RauchERP.SharedKernel;

namespace RauchERP.Modules.Identity.Domain;

public sealed class User : AggregateRoot
{
    private User() { }
    private User(Guid id, string email, string displayName, string passwordHash, DateTimeOffset now)
    { Id = id; Email = email; DisplayName = displayName; PasswordHash = passwordHash; CreatedAt = now; UpdatedAt = now; IsActive = true; }
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public static User Create(string email, string displayName, string passwordHash, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(displayName)) throw new DomainException("Email and display name are required.");
        return new(Guid.NewGuid(), email.Trim().ToLowerInvariant(), displayName.Trim(), passwordHash, now);
    }
    public void SetActive(bool active, DateTimeOffset now) { IsActive = active; UpdatedAt = now; }
}

public sealed class Role : Entity
{
    private Role() { }
    public Role(string name) { Id = Guid.NewGuid(); Name = name.Trim(); }
    public string Name { get; private set; } = string.Empty;
}

public sealed class Permission : Entity
{
    private Permission() { }
    public Permission(string code) { Id = Guid.NewGuid(); Code = code; }
    public string Code { get; private set; } = string.Empty;
}

public sealed class UserRole { public Guid UserId { get; init; } public Guid RoleId { get; init; } }
public sealed class RolePermission { public Guid RoleId { get; init; } public Guid PermissionId { get; init; } }

public sealed class UserSession : Entity
{
    private UserSession() { }
    public UserSession(Guid userId, string tokenHash, DateTimeOffset createdAt, DateTimeOffset expiresAt, string? ipAddress)
    { Id = Guid.NewGuid(); UserId = userId; TokenHash = tokenHash; CreatedAt = createdAt; ExpiresAt = expiresAt; IpAddress = ipAddress; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public Guid? ReplacedBySessionId { get; private set; }
    public string? IpAddress { get; private set; }
    public bool IsValid(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;
    public void Revoke(DateTimeOffset now, Guid? replacementId = null) { if (RevokedAt is not null) throw new DomainException("Session is already revoked."); RevokedAt = now; ReplacedBySessionId = replacementId; }
}
