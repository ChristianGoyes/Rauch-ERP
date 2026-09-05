using Microsoft.EntityFrameworkCore;
using RauchERP.Modules.Identity.Domain;

namespace RauchERP.Modules.Identity.Infrastructure;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserSession> Sessions => Set<UserSession>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("identity");
        builder.Entity<User>(b => { b.ToTable("users"); b.HasKey(x => x.Id); b.Property(x => x.Email).HasMaxLength(320); b.HasIndex(x => x.Email).IsUnique(); b.Property(x => x.DisplayName).HasMaxLength(160); b.Property(x => x.PasswordHash).HasMaxLength(255); });
        builder.Entity<Role>(b => { b.ToTable("roles"); b.HasKey(x => x.Id); b.Property(x => x.Name).HasMaxLength(100); b.HasIndex(x => x.Name).IsUnique(); });
        builder.Entity<Permission>(b => { b.ToTable("permissions"); b.HasKey(x => x.Id); b.Property(x => x.Code).HasMaxLength(160); b.HasIndex(x => x.Code).IsUnique(); });
        builder.Entity<UserRole>(b => { b.ToTable("user_roles"); b.HasKey(x => new { x.UserId, x.RoleId }); });
        builder.Entity<RolePermission>(b => { b.ToTable("role_permissions"); b.HasKey(x => new { x.RoleId, x.PermissionId }); });
        builder.Entity<UserSession>(b => { b.ToTable("user_sessions"); b.HasKey(x => x.Id); b.Property(x => x.TokenHash).HasMaxLength(64); b.HasIndex(x => x.TokenHash).IsUnique(); });
    }
}
