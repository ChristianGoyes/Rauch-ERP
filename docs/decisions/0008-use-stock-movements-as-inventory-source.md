# ADR 0008: Movimientos como fuente del inventario

Estado: Aceptado. Todo cambio crea un movimiento inmutable y actualiza el saldo en la misma transacción. Las correcciones agregan movimientos; nunca editan confirmados. Transferencias y conteos se añadirán con sus propias líneas y movimientos compensados.
