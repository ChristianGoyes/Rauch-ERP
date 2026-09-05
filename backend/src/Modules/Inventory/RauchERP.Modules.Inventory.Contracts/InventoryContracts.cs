namespace RauchERP.Modules.Inventory.Contracts;
public enum StockMovementType { InitialStock, PositiveAdjustment, NegativeAdjustment, PurchaseReceipt, Sale, SaleReturn, PurchaseReturn, TransferIn, TransferOut }
public sealed record StockAdjustmentRequest(Guid CompanyId, Guid BranchId, Guid WarehouseId, Guid ProductVariantId, decimal Quantity, decimal UnitCost, string Reason);
public sealed record StockBalanceResponse(Guid CompanyId, Guid WarehouseId, Guid ProductVariantId, decimal QuantityOnHand, decimal AverageUnitCost, DateTimeOffset UpdatedAt);
public sealed record StockMovementResponse(Guid Id, Guid WarehouseId, Guid ProductVariantId, StockMovementType MovementType, decimal Quantity, decimal UnitCost, decimal PreviousBalance, decimal NewBalance, string Reason, DateTimeOffset OccurredAt, Guid CreatedBy);
public sealed record InventoryItemResponse(Guid ProductVariantId, Guid ProductId, string ProductName, string VariantName, string Sku, string? Barcode, string UnitOfMeasure, decimal QuantityOnHand, decimal AverageUnitCost, decimal SalePrice, DateTimeOffset? UpdatedAt);
public sealed record PurchaseLineRequest(Guid ProductVariantId, decimal Quantity, decimal UnitCost, decimal? NewSalePrice);
public sealed record RegisterPurchaseRequest(Guid CompanyId, Guid BranchId, Guid WarehouseId, string SupplierName, string? InvoiceNumber, DateTimeOffset ReceivedAt, IReadOnlyCollection<PurchaseLineRequest> Lines);
public sealed record PurchaseLineResponse(Guid ProductVariantId, decimal Quantity, decimal UnitCost, decimal? NewSalePrice);
public sealed record PurchaseResponse(Guid Id, Guid WarehouseId, string SupplierName, string? InvoiceNumber, decimal TotalCost, DateTimeOffset ReceivedAt, Guid CreatedBy, IReadOnlyCollection<PurchaseLineResponse> Lines);
public sealed record InventoryIssueLine(Guid ProductVariantId, decimal Quantity);
public sealed record InventoryIssueRequest(Guid CompanyId, Guid BranchId, Guid WarehouseId, Guid SaleId, IReadOnlyCollection<InventoryIssueLine> Lines);
public interface IInventoryIssueService { Task<RauchERP.SharedKernel.Result> IssueSaleAsync(InventoryIssueRequest request, CancellationToken cancellationToken); }
public sealed record InventorySummary(decimal TotalCost, int LowStockProducts);
public interface IInventoryReportingView { Task<InventorySummary> GetSummaryAsync(Guid companyId, Guid warehouseId, decimal lowStockThreshold, CancellationToken cancellationToken); }
