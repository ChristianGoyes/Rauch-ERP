# Instrucciones para agentes

1. Leer primero `docs/index.md`.
2. Respetar Clean Architecture en backend y arquitectura hexagonal en frontend.
3. Aplicar Clean Code y SOLID.
4. No agregar lógica de negocio en controladores, endpoints ni componentes.
5. No acceder a DbContext desde Presentation ni consumir HttpClient desde componentes.
6. No acceder directamente a tablas, entidades internas o Infrastructure de otro módulo.
7. No agregar dependencias sin justificar; no introducir microservicios ni infraestructura distribuida.
8. Los movimientos confirmados son inmutables y el stock nunca se actualiza sin movimiento.
9. Usar `decimal` para dinero y `DateTimeOffset` UTC para transacciones.
10. Ejecutar compilación, lint y pruebas antes de finalizar.
11. Actualizar documentación si cambia la arquitectura.
12. Informar archivos modificados, comandos, supuestos y pruebas no ejecutadas.

## Comandos

```powershell
dotnet restore backend/RauchERP.slnx
dotnet build backend/RauchERP.slnx --no-restore
dotnet run --project backend/src/RauchERP.Api
dotnet test backend/RauchERP.slnx
dotnet test backend/tests/RauchERP.ArchitectureTests
cd frontend
npm ci
npm start
npm run lint
npm test -- --watch=false
npx playwright test
docker compose up --build
```
