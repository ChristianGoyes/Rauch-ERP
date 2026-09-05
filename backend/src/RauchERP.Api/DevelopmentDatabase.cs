using Microsoft.EntityFrameworkCore;
using RauchERP.Modules.Catalog.Domain;
using RauchERP.Modules.Catalog.Infrastructure;
using RauchERP.Modules.Identity.Contracts;
using RauchERP.Modules.Identity.Domain;
using RauchERP.Modules.Identity.Infrastructure;
using RauchERP.Modules.Inventory.Infrastructure;
using RauchERP.Modules.Organization.Domain;
using RauchERP.Modules.Organization.Infrastructure;
using RauchERP.Modules.Sales.Infrastructure;

internal static class DevelopmentDatabase
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        await using var scope = services.CreateAsyncScope(); var sp = scope.ServiceProvider;
        var identity = sp.GetRequiredService<IdentityDbContext>(); var organization = sp.GetRequiredService<OrganizationDbContext>(); var catalog = sp.GetRequiredService<CatalogDbContext>(); var inventory = sp.GetRequiredService<InventoryDbContext>(); var sales = sp.GetRequiredService<SalesDbContext>();
        await identity.Database.MigrateAsync(); await organization.Database.MigrateAsync(); await catalog.Database.MigrateAsync(); await inventory.Database.MigrateAsync(); await sales.Database.MigrateAsync();
        var permissionEntities = await identity.Permissions.ToListAsync(); foreach (var code in Permissions.All.Except(permissionEntities.Select(x => x.Code))) identity.Permissions.Add(new Permission(code));
        await identity.SaveChangesAsync();
        var roleDefinitions = new Dictionary<string, string[]>
        {
            [SystemRoles.Administrator] = Permissions.All,
            [SystemRoles.Sales] = ["catalog.categories.read", "catalog.products.read", "inventory.stock.read", "inventory.movements.read", "sales.read", "sales.create", "dashboard.read"],
            [SystemRoles.Accountant] = ["catalog.categories.read", "catalog.products.read", "inventory.stock.read", "inventory.movements.read", "inventory.purchases.read", "sales.read", "dashboard.read"]
        };
        var rolesByName = new Dictionary<string, Role>();
        foreach (var definition in roleDefinitions)
        {
            var role = await identity.Roles.SingleOrDefaultAsync(x => x.Name == definition.Key) ?? new Role(definition.Key);
            if (identity.Entry(role).State == EntityState.Detached) identity.Roles.Add(role);
            rolesByName[definition.Key] = role;
        }
        await identity.SaveChangesAsync();
        var allPermissions = await identity.Permissions.ToListAsync();
        foreach (var definition in roleDefinitions)
        {
            var role = rolesByName[definition.Key];
            var expected = allPermissions.Where(x => definition.Value.Contains(x.Code)).Select(x => x.Id).ToHashSet();
            var existing = await identity.RolePermissions.Where(x => x.RoleId == role.Id).ToListAsync();
            identity.RolePermissions.RemoveRange(existing.Where(x => !expected.Contains(x.PermissionId)));
            identity.RolePermissions.AddRange(expected.Except(existing.Select(x => x.PermissionId)).Select(x => new RolePermission { RoleId = role.Id, PermissionId = x }));
        }
        var adminRole = rolesByName[SystemRoles.Administrator];
        var email = configuration["Seed:AdminEmail"] ?? "admin@rauch.local"; var user = await identity.Users.SingleOrDefaultAsync(x => x.Email == email); if (user is null) { var password = configuration["Seed:AdminPassword"]; if (string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException("Seed:AdminPassword must be provided when development seed is enabled."); user = User.Create(email, "Rauch Administrator", BCrypt.Net.BCrypt.HashPassword(password, 12), DateTimeOffset.UtcNow); identity.Users.Add(user); }
        await identity.SaveChangesAsync(); if (!await identity.UserRoles.AnyAsync(x => x.UserId == user.Id && x.RoleId == adminRole.Id)) identity.UserRoles.Add(new() { UserId = user.Id, RoleId = adminRole.Id }); await identity.SaveChangesAsync();
        var company = await organization.Companies.SingleOrDefaultAsync(x => x.TaxIdentification == "900000001") ?? new Company("Rauch Urban Lab S.A.S.", "Rauch Urban Lab", "900000001", "1", email, "+57 300 000 0000", "Calle principal 1", "Bogotá", "CO", "COP", "America/Bogota", DateTimeOffset.UtcNow); if (organization.Entry(company).State == EntityState.Detached) organization.Companies.Add(company); await organization.SaveChangesAsync();
        var branch = await organization.Branches.SingleOrDefaultAsync(x => x.CompanyId == company.Id && x.Code == "MAIN") ?? new Branch(company.Id, "MAIN", "Principal", company.Address, company.City, company.Phone, DateTimeOffset.UtcNow); if (organization.Entry(branch).State == EntityState.Detached) organization.Branches.Add(branch); await organization.SaveChangesAsync();
        var warehouse = await organization.Warehouses.SingleOrDefaultAsync(x => x.CompanyId == company.Id && x.Code == "MAIN") ?? new Warehouse(company.Id, branch.Id, "MAIN", "Bodega principal", true, DateTimeOffset.UtcNow); if (organization.Entry(warehouse).State == EntityState.Detached) organization.Warehouses.Add(warehouse); if (!await organization.CompanyAccess.AnyAsync(x => x.UserId == user.Id && x.CompanyId == company.Id)) organization.CompanyAccess.Add(new() { UserId = user.Id, CompanyId = company.Id }); if (!await organization.BranchAccess.AnyAsync(x => x.UserId == user.Id && x.BranchId == branch.Id)) organization.BranchAccess.Add(new() { UserId = user.Id, BranchId = branch.Id }); await organization.SaveChangesAsync();
        if (!await catalog.Categories.AnyAsync(x => x.CompanyId == company.Id)) { foreach (var name in new[] { "Joyería", "Accesorios", "Smoke Shop" }) catalog.Categories.Add(Category.Create(company.Id, name, null, null, DateTimeOffset.UtcNow).Value!); await catalog.SaveChangesAsync(); }
    }
}
