using RauchERP.SharedKernel;
namespace RauchERP.Modules.Sales.Domain;
public enum SaleStatus { Pending, Confirmed, Rejected }
public sealed class Sale : AggregateRoot
{
    private Sale() { }
    public Sale(Guid companyId, Guid branchId, Guid warehouseId, string number, string customerIdentification, string customerName, string customerEmail, bool isFinalConsumer, string paymentMethod, DateTimeOffset now, Guid createdBy) { Id = Guid.NewGuid(); CompanyId = companyId; BranchId = branchId; WarehouseId = warehouseId; Number = number; CustomerIdentification = customerIdentification.Trim(); CustomerName = customerName.Trim(); CustomerEmail = customerEmail.Trim().ToLowerInvariant(); IsFinalConsumer = isFinalConsumer; PaymentMethod = paymentMethod.Trim(); Status = SaleStatus.Pending; CreatedAt = now; CreatedBy = createdBy; }
    public Guid CompanyId { get; private set; } public Guid BranchId { get; private set; } public Guid WarehouseId { get; private set; } public string Number { get; private set; } = string.Empty; public string? CustomerIdentification { get; private set; } public string? CustomerName { get; private set; } public string? CustomerEmail { get; private set; } public bool IsFinalConsumer { get; private set; } public string PaymentMethod { get; private set; } = string.Empty; public SaleStatus Status { get; private set; } public decimal Total { get; private set; } public DateTimeOffset CreatedAt { get; private set; } public Guid CreatedBy { get; private set; } public ICollection<SaleLine> Lines { get; private set; } = new List<SaleLine>();
    public void AddLine(Guid variantId, string productName, string sku, decimal quantity, decimal unitPrice) { if (quantity <= 0 || unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(quantity)); Lines.Add(new(Id, variantId, productName, sku, quantity, unitPrice)); Total += quantity * unitPrice; }
    public void Confirm() => Status = SaleStatus.Confirmed; public void Reject() => Status = SaleStatus.Rejected;
}
public sealed class SaleLine : Entity
{
    private SaleLine() { }
    public SaleLine(Guid saleId, Guid variantId, string productName, string sku, decimal quantity, decimal unitPrice) { Id = Guid.NewGuid(); SaleId = saleId; ProductVariantId = variantId; ProductName = productName; Sku = sku; Quantity = quantity; UnitPrice = unitPrice; }
    public Guid SaleId { get; private set; } public Guid ProductVariantId { get; private set; } public string ProductName { get; private set; } = string.Empty; public string Sku { get; private set; } = string.Empty; public decimal Quantity { get; private set; } public decimal UnitPrice { get; private set; }
}
