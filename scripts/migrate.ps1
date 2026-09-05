$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $projectRoot
try {
    dotnet tool restore
    dotnet ef database update -p backend/src/Modules/Identity/RauchERP.Modules.Identity.Infrastructure -s backend/src/RauchERP.Api -c IdentityDbContext
    dotnet ef database update -p backend/src/Modules/Organization/RauchERP.Modules.Organization.Infrastructure -s backend/src/RauchERP.Api -c OrganizationDbContext
    dotnet ef database update -p backend/src/Modules/Catalog/RauchERP.Modules.Catalog.Infrastructure -s backend/src/RauchERP.Api -c CatalogDbContext
    dotnet ef database update -p backend/src/Modules/Inventory/RauchERP.Modules.Inventory.Infrastructure -s backend/src/RauchERP.Api -c InventoryDbContext
}
finally { Pop-Location }
