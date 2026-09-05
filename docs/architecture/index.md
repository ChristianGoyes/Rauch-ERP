# Arquitectura

Rauch ERP es un monolito modular en monorepo. Cada módulo contiene Domain, Application, Contracts, Infrastructure y Presentation. Presentation depende de Application; Application depende de Domain y contratos; Infrastructure implementa puertos de Application. Domain no conoce ASP.NET Core, EF Core ni PostgreSQL.

Los módulos no consultan DbContext ajenos. Inventory valida variantes y bodegas mediante `IProductVariantInventoryView`, `IWarehouseInventoryView` e `IOrganizationAccessView`. El host API es el composition root.

El frontend aplica flujo `Component → Facade → Port → HTTP Adapter`. Domain y Application no importan Angular HTTP; los componentes no usan HttpClient ni almacenamiento web directamente.

Outbox se incorporará más adelante entre persistencia de eventos de dominio y dispatch de integraciones, sin modificar agregados. No forma parte de esta fase.
