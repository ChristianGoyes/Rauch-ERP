namespace RauchERP.Modules.Sales.Contracts;
public sealed record SaleLineRequest(Guid ProductVariantId, decimal Quantity, decimal UnitPrice);
public sealed record CreateSaleRequest(Guid CompanyId, Guid BranchId, Guid WarehouseId, string CustomerIdentification, string CustomerName, string CustomerEmail, bool IsFinalConsumer, string PaymentMethod, IReadOnlyCollection<SaleLineRequest> Lines);
public sealed record SaleLineResponse(Guid ProductVariantId, string ProductName, string Sku, decimal Quantity, decimal UnitPrice, decimal LineTotal);
public sealed record SaleResponse(Guid Id, Guid WarehouseId, string Number, string? CustomerIdentification, string? CustomerName, string? CustomerEmail, bool IsFinalConsumer, string PaymentMethod, string Status, decimal Total, DateTimeOffset CreatedAt, Guid CreatedBy, IReadOnlyCollection<SaleLineResponse> Lines);
public sealed record DashboardSummaryResponse(decimal TodaySales, int TodayTransactions, decimal InventoryCost, int LowStockProducts, IReadOnlyCollection<SaleResponse> RecentSales);
