# Base de datos

PostgreSQL organiza tablas por schema propietario: `identity`, `organization`, `catalog` e `inventory`. Cada módulo posee su DbContext y migración inicial. Las claves empresariales incluyen índices únicos para email, NIT, códigos, SKU, barcode y saldo por `CompanyId + WarehouseId + ProductVariantId`.

Dinero y cantidades usan `numeric(19,4)`. Fechas transaccionales son `DateTimeOffset`. Inventory usa `Version` como concurrency token; saldo y movimiento se guardan en una transacción. Ante conflicto se rechaza la operación para reintento, evitando lost updates.

El seed de desarrollo crea Rauch Urban Lab, sede y bodega principal, administrador, rol, permisos y categorías. No corre en Production y la contraseña llega por `Seed__AdminPassword`.
