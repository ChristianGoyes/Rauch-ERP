using RauchERP.Modules.Inventory.Contracts;
using RauchERP.SharedKernel;

namespace RauchERP.Modules.Inventory.Application;
public interface IInventoryService
{
    Task<Result<StockMovementResponse>> RegisterInitialAsync(StockAdjustmentRequest request, CancellationToken cancellationToken);
    Task<Result<StockMovementResponse>> AdjustPositiveAsync(StockAdjustmentRequest request, CancellationToken cancellationToken);
    Task<Result<StockMovementResponse>> AdjustNegativeAsync(StockAdjustmentRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<StockBalanceResponse>> GetWarehouseStockAsync(Guid warehouseId, CancellationToken cancellationToken);
    Task<IReadOnlyList<StockBalanceResponse>> GetVariantStockAsync(Guid variantId, CancellationToken cancellationToken);
    Task<IReadOnlyList<StockMovementResponse>> GetKardexAsync(Guid warehouseId, Guid? variantId, int page, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<StockBalanceResponse>> GetLowStockAsync(Guid warehouseId, decimal threshold, CancellationToken cancellationToken);
    Task<IReadOnlyList<InventoryItemResponse>> GetInventoryItemsAsync(Guid warehouseId, CancellationToken cancellationToken);
    Task<Result<PurchaseResponse>> RegisterPurchaseAsync(RegisterPurchaseRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<PurchaseResponse>> GetPurchasesAsync(Guid warehouseId, CancellationToken cancellationToken);
}
