# API v1

Rutas: `/api/v1/auth`, `/users`, `/roles`, `/companies`, `/branches`, `/warehouses`, `/categories`, `/brands`, `/products` e `/inventory`. OpenAPI se publica en Development en `/openapi/v1.json`; salud en `/health`.

Los errores usan RFC Problem Details. Las solicitudes autenticadas usan Bearer JWT y, para datos empresariales, `X-Company-Id` y `X-Branch-Id`. El servidor comprueba contexto, acceso y pertenencia; no confía únicamente en IDs del cliente. `X-Correlation-ID` se conserva o genera en cada solicitud.

El access token dura 15 minutos por defecto. Los refresh tokens son aleatorios, se almacenan sólo como SHA-256, rotan al renovarse y son revocables. El frontend conserva la sesión en `sessionStorage` (no LocalStorage) y la elimina al cerrar sesión; una evolución recomendada es mover refresh token a cookie HttpOnly cuando frontend y API compartan política de dominio definitiva.
