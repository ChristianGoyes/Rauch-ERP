# Rauch ERP

Base de producto para un ERP de retail especializado, implementada como monolito modular en un monorepo.

## Estado

La aplicación cubre Identity, Organization, Catalog, Inventory, compras y Sales. Incluye perfiles Administrador, Ventas y Contador; imágenes de producto; compras que actualizan stock/costos; ventas que descuentan inventario; kardex e indicadores generales. El backend usa .NET 10, ASP.NET Core, EF Core y PostgreSQL; el frontend usa Angular con componentes standalone, signals, formularios reactivos y adaptadores HTTP.

## Inicio rápido

1. Copie `.env.example` a `.env` y cambie los valores locales.
2. Ejecute `docker compose up --build`.
3. Abra `http://localhost:4200`; OpenAPI está en `http://localhost:8080/openapi/v1.json`.

Sin Docker:

```powershell
dotnet restore backend/RauchERP.slnx
dotnet run --project backend/src/RauchERP.Api
cd frontend
npm ci
npm start
```

Consulte [docs/index.md](docs/index.md) para arquitectura, migraciones, seeds y pruebas.
