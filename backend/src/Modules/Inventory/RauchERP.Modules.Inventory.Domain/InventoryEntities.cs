using RauchERP.SharedKernel;

namespace RauchERP.Modules.Inventory.Domain;

public enum StockMovementType { InitialStock, PositiveAdjustment, NegativeAdjustment, PurchaseReceipt, Sale, SaleReturn, PurchaseReturn, TransferIn, TransferOut }

public sealed class StockBalance : AggregateRoot
{
    private StockBalance() { }
    public StockBalance(Guid companyId, Guid warehouseId, Guid variantId, DateTimeOffset now) { Id = Guid.NewGuid(); CompanyId = companyId; WarehouseId = warehouseId; ProductVariantId = variantId; UpdatedAt = now; }
    public Guid CompanyId { get; private set; } public Guid WarehouseId { get; private set; } public Guid ProductVariantId { get; private set; } public decimal QuantityOnHand { get; private set; } public decimal AverageUnitCost { get; private set; } public DateTimeOffset UpdatedAt { get; private set; } public long Version { get; private set; }
    public Result<BalanceChange> Apply(StockMovementType type, decimal quantity, decimal unitCost, DateTimeOffset now)
    {
        if (quantity <= 0) return Result<BalanceChange>.Failure(new("inventory.quantity_invalid", "Quantity must be greater than zero.")); if (unitCost < 0) return Result<BalanceChange>.Failure(new("inventory.unit_cost_invalid", "Unit cost cannot be negative."));
        var previous = QuantityOnHand; var adds = type is StockMovementType.InitialStock or StockMovementType.PositiveAdjustment or StockMovementType.PurchaseReceipt or StockMovementType.SaleReturn or StockMovementType.TransferIn; var next = adds ? previous + quantity : previous - quantity; if (next < 0) return Result<BalanceChange>.Failure(new("inventory.negative_stock", "The movement would produce negative stock."));
        if (adds) AverageUnitCost = next == 0 ? 0 : ((previous * AverageUnitCost) + (quantity * unitCost)) / next; QuantityOnHand = next; UpdatedAt = now; Version++; return Result<BalanceChange>.Success(new(previous, next));
    }
}
public sealed record BalanceChange(decimal Previous, decimal Current);
public sealed class StockMovement : Entity
{
    private StockMovement() { }
    public StockMovement(Guid companyId, Guid branchId, Guid warehouseId, Guid variantId, StockMovementType type, decimal quantity, decimal unitCost, decimal previous, decimal current, string referenceType, Guid? referenceId, string reason, DateTimeOffset occurredAt, Guid createdBy) { Id = Guid.NewGuid(); CompanyId = companyId; BranchId = branchId; WarehouseId = warehouseId; ProductVariantId = variantId; MovementType = type; Quantity = quantity; UnitCost = unitCost; PreviousBalance = previous; NewBalance = current; ReferenceType = referenceType; ReferenceId = referenceId; Reason = reason; OccurredAt = occurredAt; CreatedBy = createdBy; }
    public Guid CompanyId { get; private set; } public Guid BranchId { get; private set; } public Guid WarehouseId { get; private set; } public Guid ProductVariantId { get; private set; } public StockMovementType MovementType { get; private set; } public decimal Quantity { get; private set; } public decimal UnitCost { get; private set; } public decimal PreviousBalance { get; private set; } public decimal NewBalance { get; private set; } public string ReferenceType { get; private set; } = string.Empty; public Guid? ReferenceId { get; private set; } public string Reason { get; private set; } = string.Empty; public DateTimeOffset OccurredAt { get; private set; } public Guid CreatedBy { get; private set; }
}
public sealed class StockAdjustment : AggregateRoot { public Guid CompanyId { get; init; } public Guid WarehouseId { get; init; } public DateTimeOffset CreatedAt { get; init; } public Guid CreatedBy { get; init; } }
public sealed class StockAdjustmentLine : Entity { public Guid StockAdjustmentId { get; init; } public Guid ProductVariantId { get; init; } public decimal Quantity { get; init; } public string Reason { get; init; } = string.Empty; }
public sealed class PurchaseReceipt : AggregateRoot
{
    private PurchaseReceipt() { }
    public PurchaseReceipt(Guid companyId, Guid branchId, Guid warehouseId, string supplierName, string? invoiceNumber, DateTimeOffset receivedAt, Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(supplierName)) throw new ArgumentException("Supplier is required.", nameof(supplierName));
        Id = Guid.NewGuid(); CompanyId = companyId; BranchId = branchId; WarehouseId = warehouseId; SupplierName = supplierName.Trim(); InvoiceNumber = invoiceNumber?.Trim(); ReceivedAt = receivedAt; CreatedBy = createdBy;
    }
    public Guid CompanyId { get; private set; } public Guid BranchId { get; private set; } public Guid WarehouseId { get; private set; } public string SupplierName { get; private set; } = string.Empty; public string? InvoiceNumber { get; private set; } public DateTimeOffset ReceivedAt { get; private set; } public Guid CreatedBy { get; private set; }
    public ICollection<PurchaseReceiptLine> Lines { get; private set; } = new List<PurchaseReceiptLine>();
    public void AddLine(Guid variantId, decimal quantity, decimal unitCost, decimal? newSalePrice) { if (quantity <= 0 || unitCost < 0 || newSalePrice < 0) throw new ArgumentOutOfRangeException(nameof(quantity)); Lines.Add(new(Id, variantId, quantity, unitCost, newSalePrice)); }
}
public sealed class PurchaseReceiptLine : Entity
{
    private PurchaseReceiptLine() { }
    public PurchaseReceiptLine(Guid receiptId, Guid variantId, decimal quantity, decimal unitCost, decimal? newSalePrice) { Id = Guid.NewGuid(); PurchaseReceiptId = receiptId; ProductVariantId = variantId; Quantity = quantity; UnitCost = unitCost; NewSalePrice = newSalePrice; }
    public Guid PurchaseReceiptId { get; private set; } public Guid ProductVariantId { get; private set; } public decimal Quantity { get; private set; } public decimal UnitCost { get; private set; } public decimal? NewSalePrice { get; private set; }
}
