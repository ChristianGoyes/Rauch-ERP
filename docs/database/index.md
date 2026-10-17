# Base de datos

PostgreSQL organiza tablas por schema propietario: `identity`, `organization`, `catalog` e `inventory`. Cada módulo posee su DbContext y migración inicial. Las claves empresariales incluyen índices únicos para email, NIT, códigos, SKU, barcode y saldo por `CompanyId + WarehouseId + ProductVariantId`.

Dinero y cantidades usan `numeric(19,4)`. Fechas transaccionales son `DateTimeOffset`. Inventory usa `Version` como concurrency token; saldo y movimiento se guardan en una transacción. Ante conflicto se rechaza la operación para reintento, evitando lost updates.

El seed de desarrollo crea Rauch Urban Lab, sede y bodega principal, administrador, rol, permisos y categorías. No corre en Production y la contraseña llega por `Seed__AdminPassword`.

## Clasificación del entorno actual

La instancia PostgreSQL local `localhost:5432/rauch_erp` es la base oficial de **QA/pruebas** durante el desarrollo. Puede contener datos ficticios, usuarios con credenciales compartidas y transacciones repetibles. Nunca debe conectarse a una publicación de producción ni restaurarse sobre ella.

La futura producción debe comenzar en una base PostgreSQL separada y vacía, por ejemplo `rauch_erp_prod`, aplicar las migraciones versionadas y usar secretos exclusivos. El seed debe permanecer desactivado con `Seed__Enabled=false` y `ASPNETCORE_ENVIRONMENT=Production`.

Los datos QA se identifican con nombres, correos, facturas y SKU que incluyen el prefijo `QA`. Las imágenes y copias de seguridad de este ambiente tampoco se deben promover a producción.
