using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RauchERP.Application.Abstractions;
using RauchERP.Modules.Catalog.Contracts;
using RauchERP.Modules.Inventory.Application;
using RauchERP.Modules.Inventory.Contracts;
using RauchERP.Modules.Inventory.Domain;
using RauchERP.Modules.Organization.Contracts;
using RauchERP.SharedKernel;
using ContractMovementType = RauchERP.Modules.Inventory.Contracts.StockMovementType;
using DomainMovementType = RauchERP.Modules.Inventory.Domain.StockMovementType;

namespace RauchERP.Modules.Inventory.Infrastructure;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<StockBalance> StockBalances => Set<StockBalance>(); public DbSet<StockMovement> StockMovements => Set<StockMovement>(); public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>(); public DbSet<StockAdjustmentLine> StockAdjustmentLines => Set<StockAdjustmentLine>(); public DbSet<PurchaseReceipt> PurchaseReceipts => Set<PurchaseReceipt>(); public DbSet<PurchaseReceiptLine> PurchaseReceiptLines => Set<PurchaseReceiptLine>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("inventory");
        b.Entity<StockBalance>(x => { x.ToTable("stock_balances"); x.HasKey(e => e.Id); x.Property(e => e.QuantityOnHand).HasPrecision(19, 4); x.Property(e => e.AverageUnitCost).HasPrecision(19, 4); x.Property(e => e.Version).IsConcurrencyToken(); x.HasIndex(e => new { e.CompanyId, e.WarehouseId, e.ProductVariantId }).IsUnique(); });
        b.Entity<StockMovement>(x => { x.ToTable("stock_movements"); x.HasKey(e => e.Id); x.Property(e => e.Quantity).HasPrecision(19, 4); x.Property(e => e.UnitCost).HasPrecision(19, 4); x.Property(e => e.PreviousBalance).HasPrecision(19, 4); x.Property(e => e.NewBalance).HasPrecision(19, 4); x.Property(e => e.ReferenceType).HasMaxLength(80); x.Property(e => e.Reason).HasMaxLength(500); x.HasIndex(e => new { e.CompanyId, e.WarehouseId, e.ProductVariantId, e.OccurredAt }); });
        b.Entity<StockAdjustment>(x => { x.ToTable("stock_adjustments"); x.HasKey(e => e.Id); }); b.Entity<StockAdjustmentLine>(x => { x.ToTable("stock_adjustment_lines"); x.HasKey(e => e.Id); x.Property(e => e.Quantity).HasPrecision(19, 4); });
        b.Entity<PurchaseReceipt>(x => { x.ToTable("purchase_receipts"); x.HasKey(e => e.Id); x.Property(e => e.SupplierName).HasMaxLength(200); x.Property(e => e.InvoiceNumber).HasMaxLength(80); x.HasMany(e => e.Lines).WithOne().HasForeignKey(e => e.PurchaseReceiptId).OnDelete(DeleteBehavior.Cascade); x.HasIndex(e => new { e.CompanyId, e.WarehouseId, e.ReceivedAt }); });
        b.Entity<PurchaseReceiptLine>(x => { x.ToTable("purchase_receipt_lines"); x.HasKey(e => e.Id); x.Property(e => e.Quantity).HasPrecision(19, 4); x.Property(e => e.UnitCost).HasPrecision(19, 4); x.Property(e => e.NewSalePrice).HasPrecision(19, 4); });
    }
}

internal sealed class InventoryService(InventoryDbContext db, IProductVariantInventoryView products, IProductCatalogView catalog, IProductVariantPricing pricing, IWarehouseInventoryView warehouses, IOrganizationAccessView access, ICurrentUser user, ICompanyContext context) : IInventoryService, IInventoryIssueService, IInventoryReportingView
{
    public Task<Result<StockMovementResponse>> RegisterInitialAsync(StockAdjustmentRequest r, CancellationToken ct) => Apply(r, DomainMovementType.InitialStock, ct);
    public Task<Result<StockMovementResponse>> AdjustPositiveAsync(StockAdjustmentRequest r, CancellationToken ct) => Apply(r, DomainMovementType.PositiveAdjustment, ct);
    public Task<Result<StockMovementResponse>> AdjustNegativeAsync(StockAdjustmentRequest r, CancellationToken ct) => Apply(r, DomainMovementType.NegativeAdjustment, ct);
    private async Task<Result<StockMovementResponse>> Apply(StockAdjustmentRequest r, DomainMovementType type, CancellationToken ct)
    {
        if (context.CompanyId != r.CompanyId || context.BranchId != r.BranchId || user.UserId is not Guid userId) return Fail("inventory.context_invalid", "Company, branch, or user context is invalid.");
        var warehouse = await warehouses.GetAsync(r.WarehouseId, ct); if (warehouse is null || !warehouse.IsActive || warehouse.CompanyId != r.CompanyId || warehouse.BranchId != r.BranchId) return Fail("inventory.warehouse_invalid", "Warehouse is inactive or does not match the company and branch.");
        if (!await access.HasAccessAsync(userId, r.CompanyId, r.BranchId, ct)) return Fail("inventory.access_denied", "User does not have access to the company and branch.");
        var variant = await products.GetAsync(r.ProductVariantId, ct); if (variant is null || variant.CompanyId != r.CompanyId || !variant.ProductIsActive || !variant.VariantIsActive) return Fail("inventory.variant_invalid", "Variant is missing, inactive, or belongs to another company."); if (!variant.TracksInventory) return Fail("inventory.not_tracked", "The product does not track inventory.");
        await using var tx = await db.Database.BeginTransactionAsync(ct); var balance = await db.StockBalances.SingleOrDefaultAsync(x => x.CompanyId == r.CompanyId && x.WarehouseId == r.WarehouseId && x.ProductVariantId == r.ProductVariantId, ct) ?? new StockBalance(r.CompanyId, r.WarehouseId, r.ProductVariantId, DateTimeOffset.UtcNow);
        if (db.Entry(balance).State == EntityState.Detached) db.StockBalances.Add(balance); if (type == DomainMovementType.InitialStock && (balance.QuantityOnHand != 0 || await db.StockMovements.AnyAsync(x => x.WarehouseId == r.WarehouseId && x.ProductVariantId == r.ProductVariantId, ct))) return Fail("inventory.initial_exists", "Initial stock has already been registered.");
        var changed = balance.Apply(type, r.Quantity, r.UnitCost, DateTimeOffset.UtcNow); if (changed.IsFailure) return Result<StockMovementResponse>.Failure(changed.Error); var movement = new StockMovement(r.CompanyId, r.BranchId, r.WarehouseId, r.ProductVariantId, type, r.Quantity, r.UnitCost, changed.Value!.Previous, changed.Value.Current, type.ToString(), null, r.Reason, DateTimeOffset.UtcNow, userId); db.StockMovements.Add(movement);
        try { await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return Result<StockMovementResponse>.Success(Map(movement)); } catch (DbUpdateConcurrencyException) { await tx.RollbackAsync(ct); return Fail("inventory.concurrent_update", "Stock changed concurrently. Retry the operation."); }
    }
    public async Task<IReadOnlyList<StockBalanceResponse>> GetWarehouseStockAsync(Guid warehouseId, CancellationToken ct) => await db.StockBalances.AsNoTracking().Where(x => x.WarehouseId == warehouseId && x.CompanyId == context.CompanyId).Select(x => Map(x)).ToListAsync(ct);
    public async Task<IReadOnlyList<StockBalanceResponse>> GetVariantStockAsync(Guid variantId, CancellationToken ct) => await db.StockBalances.AsNoTracking().Where(x => x.ProductVariantId == variantId && x.CompanyId == context.CompanyId).Select(x => Map(x)).ToListAsync(ct);
    public async Task<IReadOnlyList<StockMovementResponse>> GetKardexAsync(Guid warehouseId, Guid? variantId, int page, int pageSize, CancellationToken ct) { var q = db.StockMovements.AsNoTracking().Where(x => x.WarehouseId == warehouseId && x.CompanyId == context.CompanyId); if (variantId is Guid id) q = q.Where(x => x.ProductVariantId == id); return await q.OrderByDescending(x => x.OccurredAt).Skip((Math.Max(1, page) - 1) * Math.Clamp(pageSize, 1, 100)).Take(Math.Clamp(pageSize, 1, 100)).Select(x => Map(x)).ToListAsync(ct); }
    public async Task<IReadOnlyList<StockBalanceResponse>> GetLowStockAsync(Guid warehouseId, decimal threshold, CancellationToken ct) => await db.StockBalances.AsNoTracking().Where(x => x.WarehouseId == warehouseId && x.CompanyId == context.CompanyId && x.QuantityOnHand <= threshold).OrderBy(x => x.QuantityOnHand).Select(x => Map(x)).ToListAsync(ct);
    public async Task<IReadOnlyList<InventoryItemResponse>> GetInventoryItemsAsync(Guid warehouseId, CancellationToken ct)
    {
        var variants = await catalog.GetVariantsAsync(context.CompanyId ?? Guid.Empty, ct); var balances = await db.StockBalances.AsNoTracking().Where(x => x.WarehouseId == warehouseId && x.CompanyId == context.CompanyId).ToDictionaryAsync(x => x.ProductVariantId, ct);
        return variants.Where(x => x.TracksInventory && x.IsActive).Select(x => { balances.TryGetValue(x.Id, out var b); return new InventoryItemResponse(x.Id, x.ProductId, x.ProductName, x.VariantName, x.Sku, x.Barcode, x.UnitOfMeasure, b?.QuantityOnHand ?? 0, b?.AverageUnitCost ?? x.Cost, x.SalePrice, b?.UpdatedAt); }).OrderBy(x => x.ProductName).ThenBy(x => x.VariantName).ToList();
    }
    public async Task<Result<PurchaseResponse>> RegisterPurchaseAsync(RegisterPurchaseRequest r, CancellationToken ct)
    {
        var validation = await ValidateContext(r.CompanyId, r.BranchId, r.WarehouseId, ct); if (validation.IsFailure) return Result<PurchaseResponse>.Failure(validation.Error);
        if (r.Lines.Count == 0) return Result<PurchaseResponse>.Failure(new("inventory.purchase_empty", "Add at least one product to the purchase."));
        if (r.Lines.GroupBy(x => x.ProductVariantId).Any(x => x.Count() > 1)) return Result<PurchaseResponse>.Failure(new("inventory.purchase_duplicate", "A product variant can only appear once per purchase."));
        var receipt = new PurchaseReceipt(r.CompanyId, r.BranchId, r.WarehouseId, r.SupplierName, r.InvoiceNumber, r.ReceivedAt, user.UserId!.Value);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        foreach (var line in r.Lines)
        {
            var variant = await products.GetAsync(line.ProductVariantId, ct); if (variant is null || variant.CompanyId != r.CompanyId || !variant.ProductIsActive || !variant.VariantIsActive || !variant.TracksInventory) return Result<PurchaseResponse>.Failure(new("inventory.variant_invalid", "One of the selected products is inactive, missing, or does not track inventory."));
            if (line.Quantity <= 0 || line.UnitCost < 0 || line.NewSalePrice < 0) return Result<PurchaseResponse>.Failure(new("inventory.purchase_line_invalid", "Quantity and prices in a purchase must be valid."));
            receipt.AddLine(line.ProductVariantId, line.Quantity, line.UnitCost, line.NewSalePrice);
            var balance = await db.StockBalances.SingleOrDefaultAsync(x => x.CompanyId == r.CompanyId && x.WarehouseId == r.WarehouseId && x.ProductVariantId == line.ProductVariantId, ct) ?? new StockBalance(r.CompanyId, r.WarehouseId, line.ProductVariantId, r.ReceivedAt);
            if (db.Entry(balance).State == EntityState.Detached) db.StockBalances.Add(balance);
            var changed = balance.Apply(DomainMovementType.PurchaseReceipt, line.Quantity, line.UnitCost, r.ReceivedAt); if (changed.IsFailure) return Result<PurchaseResponse>.Failure(changed.Error);
            db.StockMovements.Add(new(r.CompanyId, r.BranchId, r.WarehouseId, line.ProductVariantId, DomainMovementType.PurchaseReceipt, line.Quantity, line.UnitCost, changed.Value!.Previous, changed.Value.Current, "PurchaseReceipt", receipt.Id, $"Compra {r.InvoiceNumber ?? receipt.Id.ToString("N")}", r.ReceivedAt, user.UserId.Value));
        }
        db.PurchaseReceipts.Add(receipt);
        try { await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); } catch (DbUpdateConcurrencyException) { await tx.RollbackAsync(ct); return Result<PurchaseResponse>.Failure(new("inventory.concurrent_update", "Stock changed concurrently. Retry the purchase.")); }
        foreach (var line in r.Lines) await pricing.UpdatePricingAsync(line.ProductVariantId, line.UnitCost, line.NewSalePrice, ct);
        return Result<PurchaseResponse>.Success(Map(receipt));
    }
    public async Task<IReadOnlyList<PurchaseResponse>> GetPurchasesAsync(Guid warehouseId, CancellationToken ct) { var rows = await db.PurchaseReceipts.AsNoTracking().Include(x => x.Lines).Where(x => x.CompanyId == context.CompanyId && x.WarehouseId == warehouseId).OrderByDescending(x => x.ReceivedAt).ToListAsync(ct); return rows.Select(Map).ToList(); }
    public async Task<Result> IssueSaleAsync(InventoryIssueRequest r, CancellationToken ct)
    {
        var validation = await ValidateContext(r.CompanyId, r.BranchId, r.WarehouseId, ct); if (validation.IsFailure) return validation;
        if (r.Lines.Count == 0) return Result.Failure(new("inventory.sale_empty", "The sale has no products."));
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        foreach (var line in r.Lines)
        {
            if (line.Quantity <= 0) return Result.Failure(new("inventory.quantity_invalid", "Sale quantity must be greater than zero."));
            var balance = await db.StockBalances.SingleOrDefaultAsync(x => x.CompanyId == r.CompanyId && x.WarehouseId == r.WarehouseId && x.ProductVariantId == line.ProductVariantId, ct); if (balance is null) return Result.Failure(new("inventory.stock_missing", "A product has no stock in the selected warehouse."));
            var changed = balance.Apply(DomainMovementType.Sale, line.Quantity, balance.AverageUnitCost, DateTimeOffset.UtcNow); if (changed.IsFailure) return Result.Failure(changed.Error);
            db.StockMovements.Add(new(r.CompanyId, r.BranchId, r.WarehouseId, line.ProductVariantId, DomainMovementType.Sale, line.Quantity, balance.AverageUnitCost, changed.Value!.Previous, changed.Value.Current, "Sale", r.SaleId, $"Venta {r.SaleId:N}", DateTimeOffset.UtcNow, user.UserId!.Value));
        }
        try { await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return Result.Success(); } catch (DbUpdateConcurrencyException) { await tx.RollbackAsync(ct); return Result.Failure(new("inventory.concurrent_update", "Stock changed concurrently. Retry the sale.")); }
    }
    private async Task<Result> ValidateContext(Guid companyId, Guid branchId, Guid warehouseId, CancellationToken ct)
    {
        if (context.CompanyId != companyId || context.BranchId != branchId || user.UserId is not Guid userId) return Result.Failure(new("inventory.context_invalid", "Company, branch, or user context is invalid."));
        var warehouse = await warehouses.GetAsync(warehouseId, ct); if (warehouse is null || !warehouse.IsActive || warehouse.CompanyId != companyId || warehouse.BranchId != branchId) return Result.Failure(new("inventory.warehouse_invalid", "Warehouse is inactive or does not match the company and branch."));
        return await access.HasAccessAsync(userId, companyId, branchId, ct) ? Result.Success() : Result.Failure(new("inventory.access_denied", "User does not have access to the company and branch."));
    }
    public async Task<InventorySummary> GetSummaryAsync(Guid companyId, Guid warehouseId, decimal threshold, CancellationToken ct) { var balances = await db.StockBalances.AsNoTracking().Where(x => x.CompanyId == companyId && x.WarehouseId == warehouseId).ToListAsync(ct); return new(balances.Sum(x => x.QuantityOnHand * x.AverageUnitCost), balances.Count(x => x.QuantityOnHand <= threshold)); }
    private static Result<StockMovementResponse> Fail(string code, string message) => Result<StockMovementResponse>.Failure(new(code, message));
    private static StockBalanceResponse Map(StockBalance x) => new(x.CompanyId, x.WarehouseId, x.ProductVariantId, x.QuantityOnHand, x.AverageUnitCost, x.UpdatedAt);
    private static StockMovementResponse Map(StockMovement x) => new(x.Id, x.WarehouseId, x.ProductVariantId, Enum.Parse<ContractMovementType>(x.MovementType.ToString()), x.Quantity, x.UnitCost, x.PreviousBalance, x.NewBalance, x.Reason, x.OccurredAt, x.CreatedBy);
    private static PurchaseResponse Map(PurchaseReceipt x) => new(x.Id, x.WarehouseId, x.SupplierName, x.InvoiceNumber, x.Lines.Sum(l => l.Quantity * l.UnitCost), x.ReceivedAt, x.CreatedBy, x.Lines.Select(l => new PurchaseLineResponse(l.ProductVariantId, l.Quantity, l.UnitCost, l.NewSalePrice)).ToList());
}
public static class InventoryModule { public static IServiceCollection AddInventoryModule(this IServiceCollection s, IConfiguration c) { s.AddDbContext<InventoryDbContext>(o => o.UseNpgsql(c.GetConnectionString("Database"))); s.AddScoped<InventoryService>(); s.AddScoped<IInventoryService>(sp => sp.GetRequiredService<InventoryService>()); s.AddScoped<IInventoryIssueService>(sp => sp.GetRequiredService<InventoryService>()); s.AddScoped<IInventoryReportingView>(sp => sp.GetRequiredService<InventoryService>()); return s; } }
