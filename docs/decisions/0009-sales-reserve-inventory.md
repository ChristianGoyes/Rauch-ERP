# ADR 0009: confirmar ventas mediante egreso de inventario

## Estado

Aceptado.

## Decisión

Una venta se crea en estado pendiente. Inventory valida y registra todos sus egresos en una transacción; sólo entonces Sales la confirma. Si falta stock, la venta queda rechazada y no se registra ningún egreso parcial.

Sales e Inventory se comunican mediante contratos, sin acceder directamente a las tablas del otro módulo. Al seguir siendo un monolito local, una fase posterior incorporará outbox y reconciliación para cubrir fallos excepcionales entre las dos confirmaciones de base de datos.
