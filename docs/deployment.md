# Requisitos de despliegue

## Infraestructura mínima

- Un servidor Linux o plataforma de contenedores con Docker y Docker Compose. Como punto de partida: 2 CPU, 4 GB de RAM y 20 GB de almacenamiento, ajustables después de medir uso real.
- PostgreSQL 17 administrado o en un contenedor con volumen persistente. Producción debe usar una base nueva y separada de `rauch_erp` local.
- Dominio propio, DNS y certificado HTTPS. Nginx, Caddy o el balanceador de la plataforma debe terminar TLS y redirigir HTTP a HTTPS.
- Almacenamiento persistente para `wwwroot/uploads/products`. Antes de escalar a varias instancias conviene mover las imágenes a almacenamiento de objetos compatible con S3.
- Copias automáticas de PostgreSQL, retención definida y una prueba periódica de restauración.

## Configuración obligatoria

- `ASPNETCORE_ENVIRONMENT=Production`.
- `ConnectionStrings__Database` con usuario exclusivo, contraseña fuerte y TLS cuando el proveedor lo soporte.
- `Jwt__Issuer`, `Jwt__Audience` y `Jwt__SigningKey`; la clave debe ser aleatoria, larga y administrada como secreto.
- `Cors__AllowedOrigins__0=https://erp.sudominio.com`.
- `Seed__Enabled=false`. El administrador inicial de producción debe crearse mediante un procedimiento controlado, no con las credenciales QA.
- Persistencia de claves de ASP.NET Data Protection fuera del contenedor si se ejecuta más de una instancia o se recrean contenedores.

## Publicación

1. Construir imágenes de backend y frontend desde un commit etiquetado.
2. Crear la base de producción vacía y una copia de seguridad inicial.
3. Aplicar las migraciones de Identity, Organization, Catalog, Inventory y Sales antes de enrutar tráfico.
4. Publicar backend y frontend, comprobar `/health`, inicio de sesión y una operación controlada.
5. Activar monitoreo de disponibilidad, errores, espacio, CPU, memoria y conexiones PostgreSQL.
6. Configurar CI/CD para ejecutar build, lint, pruebas unitarias, arquitectura, funcionales y E2E antes del despliegue.

## Pendientes antes de producción

- Crear un `compose` o manifiesto específico de producción: el actual usa `Development` y activa el seed.
- Externalizar las imágenes de producto o montar un volumen persistente con respaldo.
- Incorporar auditoría de cambios sensibles, política de contraseñas/recuperación, rate limiting y rotación de secretos.
- Añadir monitoreo centralizado y alertas, además de una estrategia de actualización y rollback.
- Definir requisitos legales y fiscales colombianos, especialmente numeración, impuestos y facturación electrónica DIAN, antes de usar ventas reales.
