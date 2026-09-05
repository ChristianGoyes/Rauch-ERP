# Documentación de Rauch ERP

- [Arquitectura](architecture/index.md)
- [Módulos](modules/index.md)
- [Base de datos](database/index.md)
- [API](api/index.md)
- [Decisiones](decisions/index.md)
- [Recomendaciones y próximos pasos](roadmap.md)

## Ejecución local

Requisitos: .NET SDK 10.0.204+, Node 22.16+ y PostgreSQL 17. Configure `ConnectionStrings__Database`, `Jwt__SigningKey` y, si activa el seed, `Seed__AdminPassword`.

```powershell
dotnet tool restore
dotnet restore backend/RauchERP.slnx
dotnet ef database update -p backend/src/Modules/Identity/RauchERP.Modules.Identity.Infrastructure -s backend/src/RauchERP.Api -c IdentityDbContext
dotnet ef database update -p backend/src/Modules/Organization/RauchERP.Modules.Organization.Infrastructure -s backend/src/RauchERP.Api -c OrganizationDbContext
dotnet ef database update -p backend/src/Modules/Catalog/RauchERP.Modules.Catalog.Infrastructure -s backend/src/RauchERP.Api -c CatalogDbContext
dotnet ef database update -p backend/src/Modules/Inventory/RauchERP.Modules.Inventory.Infrastructure -s backend/src/RauchERP.Api -c InventoryDbContext
dotnet ef database update -p backend/src/Modules/Sales/RauchERP.Modules.Sales.Infrastructure -s backend/src/RauchERP.Api -c SalesDbContext
$env:Seed__AdminPassword='una-clave-local-segura'
dotnet run --project backend/src/RauchERP.Api
cd frontend
npm ci
npm start
```

Con Docker: copie `.env.example` como `.env`, cambie claves y ejecute `docker compose up --build`.

## Validación

```powershell
dotnet build backend/RauchERP.slnx
dotnet test backend/RauchERP.slnx
cd frontend
npm run lint
npm test -- --watch=false
npm run build
npx playwright test
```

Las pruebas de integración PostgreSQL usan Testcontainers y requieren Docker. El seed sólo se activa en Development y exige una contraseña externa.
