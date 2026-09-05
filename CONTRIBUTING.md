# Contribuir

Lea `AGENTS.md` y `docs/index.md`. Cree cambios pequeños, mantenga los límites de módulo y agregue pruebas para reglas de negocio. Antes de entregar ejecute `dotnet test backend/RauchERP.slnx` y, en `frontend`, `npm run lint`, `npm test -- --watch=false` y `npm run build`.

No confirme secretos, artefactos generados ni archivos `.env`. Los commits deben explicar el motivo del cambio y cualquier decisión arquitectónica nueva debe registrarse como ADR.
