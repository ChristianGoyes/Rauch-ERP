using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RauchERP.Modules.Inventory.Application;
using RauchERP.Modules.Inventory.Contracts;
using RauchERP.SharedKernel;

namespace RauchERP.Modules.Inventory.Presentation;
public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var inventory = app.MapGroup("/api/v1/inventory").WithTags("Inventory").RequireAuthorization();
        inventory.MapPost("/initial", async (StockAdjustmentRequest r, IInventoryService s, CancellationToken ct) => Http(await s.RegisterInitialAsync(r, ct))).RequireAuthorization("inventory.adjustments.create");
        inventory.MapPost("/adjustments/positive", async (StockAdjustmentRequest r, IInventoryService s, CancellationToken ct) => Http(await s.AdjustPositiveAsync(r, ct))).RequireAuthorization("inventory.adjustments.create");
        inventory.MapPost("/adjustments/negative", async (StockAdjustmentRequest r, IInventoryService s, CancellationToken ct) => Http(await s.AdjustNegativeAsync(r, ct))).RequireAuthorization("inventory.adjustments.create");
        inventory.MapGet("/stock/warehouse/{id:guid}", async (Guid id, IInventoryService s, CancellationToken ct) => Results.Ok(await s.GetWarehouseStockAsync(id, ct)));
        inventory.MapGet("/stock/variant/{id:guid}", async (Guid id, IInventoryService s, CancellationToken ct) => Results.Ok(await s.GetVariantStockAsync(id, ct)));
        inventory.MapGet("/kardex", async (Guid warehouseId, Guid? variantId, int page, int pageSize, IInventoryService s, CancellationToken ct) => Results.Ok(await s.GetKardexAsync(warehouseId, variantId, page, pageSize, ct)));
        inventory.MapGet("/low-stock", async (Guid warehouseId, decimal threshold, IInventoryService s, CancellationToken ct) => Results.Ok(await s.GetLowStockAsync(warehouseId, threshold, ct)));
        inventory.MapGet("/items", async (Guid warehouseId, IInventoryService s, CancellationToken ct) => Results.Ok(await s.GetInventoryItemsAsync(warehouseId, ct))).RequireAuthorization("inventory.stock.read");
        inventory.MapPost("/purchases", async (RegisterPurchaseRequest r, IInventoryService s, CancellationToken ct) => Http(await s.RegisterPurchaseAsync(r, ct))).RequireAuthorization("inventory.purchases.create");
        inventory.MapGet("/purchases", async (Guid warehouseId, IInventoryService s, CancellationToken ct) => Results.Ok(await s.GetPurchasesAsync(warehouseId, ct))).RequireAuthorization("inventory.purchases.read");
        return app;
    }
    private static IResult Http<T>(Result<T> r) => r.IsSuccess ? Results.Ok(r.Value) : Results.Problem(r.Error.Description, statusCode: 400, title: r.Error.Code);
}
