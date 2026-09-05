using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RauchERP.Application.Abstractions;
using RauchERP.Modules.Catalog.Contracts;
using RauchERP.Modules.Inventory.Contracts;
using RauchERP.Modules.Organization.Contracts;
using RauchERP.Modules.Sales.Application;
using RauchERP.Modules.Sales.Contracts;
using RauchERP.Modules.Sales.Domain;
using RauchERP.SharedKernel;

namespace RauchERP.Modules.Sales.Infrastructure;
public sealed class SalesDbContext(DbContextOptions<SalesDbContext> options) : DbContext(options)
{
    public DbSet<Sale> Sales => Set<Sale>(); public DbSet<SaleLine> SaleLines => Set<SaleLine>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("sales");
        b.Entity<Sale>(x => { x.ToTable("sales"); x.HasKey(e => e.Id); x.Property(e => e.Number).HasMaxLength(40); x.Property(e => e.CustomerIdentification).HasMaxLength(30); x.Property(e => e.CustomerName).HasMaxLength(200); x.Property(e => e.CustomerEmail).HasMaxLength(254); x.Property(e => e.PaymentMethod).HasMaxLength(40); x.Property(e => e.Total).HasPrecision(19, 4); x.HasIndex(e => new { e.CompanyId, e.Number }).IsUnique(); x.HasMany(e => e.Lines).WithOne().HasForeignKey(e => e.SaleId).OnDelete(DeleteBehavior.Cascade); });
        b.Entity<SaleLine>(x => { x.ToTable("sale_lines"); x.HasKey(e => e.Id); x.Property(e => e.ProductName).HasMaxLength(240); x.Property(e => e.Sku).HasMaxLength(80); x.Property(e => e.Quantity).HasPrecision(19, 4); x.Property(e => e.UnitPrice).HasPrecision(19, 4); x.HasIndex(e => e.ProductVariantId); });
    }
}
internal sealed class SalesService(SalesDbContext db, IInventoryIssueService inventory, IInventoryReportingView inventoryReport, IProductCatalogView catalog, IWarehouseInventoryView warehouses, IOrganizationAccessView access, ICurrentUser user, ICompanyContext context) : ISalesService
{
    public async Task<Result<SaleResponse>> CreateAsync(CreateSaleRequest r, CancellationToken ct)
    {
        if (context.CompanyId != r.CompanyId || context.BranchId != r.BranchId || user.UserId is not Guid userId) return Fail("sales.context_invalid", "Company, branch, or user context is invalid.");
        var warehouse = await warehouses.GetAsync(r.WarehouseId, ct); if (warehouse is null || !warehouse.IsActive || warehouse.CompanyId != r.CompanyId || warehouse.BranchId != r.BranchId) return Fail("sales.warehouse_invalid", "Select an active warehouse from the current branch.");
        if (!await access.HasAccessAsync(userId, r.CompanyId, r.BranchId, ct)) return Fail("sales.access_denied", "You do not have access to this company and branch.");
        if (r.Lines.Count == 0 || r.Lines.Any(x => x.Quantity <= 0 || x.UnitPrice < 0) || r.Lines.GroupBy(x => x.ProductVariantId).Any(x => x.Count() > 1)) return Fail("sales.lines_invalid", "Add valid products without duplicate variants.");
        const string finalIdentification = "222222222222"; const string finalName = "Consumidor final"; const string finalEmail = "consumidor.final@rauch.local";
        var customerIdentification = r.IsFinalConsumer ? finalIdentification : r.CustomerIdentification.Trim(); var customerName = r.IsFinalConsumer ? finalName : r.CustomerName.Trim(); var customerEmail = r.IsFinalConsumer ? finalEmail : r.CustomerEmail.Trim();
        if (string.IsNullOrWhiteSpace(customerIdentification) || string.IsNullOrWhiteSpace(customerName) || !customerEmail.Contains('@') || customerEmail.Length > 254) return Fail("sales.customer_invalid", "Identification, name, and a valid email are required for the customer.");
        var lookups = (await catalog.GetVariantsAsync(r.CompanyId, ct)).ToDictionary(x => x.Id); var now = DateTimeOffset.UtcNow; var sale = new Sale(r.CompanyId, r.BranchId, r.WarehouseId, $"V-{now:yyyyMMddHHmmssfff}", customerIdentification, customerName, customerEmail, r.IsFinalConsumer, string.IsNullOrWhiteSpace(r.PaymentMethod) ? "Efectivo" : r.PaymentMethod, now, userId);
        foreach (var line in r.Lines) { if (!lookups.TryGetValue(line.ProductVariantId, out var product) || !product.IsActive || !product.TracksInventory) return Fail("sales.product_invalid", "One selected product is inactive or unavailable."); sale.AddLine(line.ProductVariantId, $"{product.ProductName} · {product.VariantName}", product.Sku, line.Quantity, line.UnitPrice); }
        db.Sales.Add(sale); await db.SaveChangesAsync(ct);
        var issued = await inventory.IssueSaleAsync(new(r.CompanyId, r.BranchId, r.WarehouseId, sale.Id, r.Lines.Select(x => new InventoryIssueLine(x.ProductVariantId, x.Quantity)).ToList()), ct);
        if (issued.IsFailure) { sale.Reject(); await db.SaveChangesAsync(ct); return Result<SaleResponse>.Failure(issued.Error); }
        sale.Confirm(); await db.SaveChangesAsync(ct); return Result<SaleResponse>.Success(Map(sale));
    }
    public async Task<IReadOnlyList<SaleResponse>> GetAsync(Guid warehouseId, CancellationToken ct) { var rows = await db.Sales.AsNoTracking().Include(x => x.Lines).Where(x => x.CompanyId == context.CompanyId && x.WarehouseId == warehouseId).OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(ct); return rows.Select(Map).ToList(); }
    public async Task<IReadOnlyList<SaleResponse>> GetByVariantAsync(Guid variantId, CancellationToken ct) { var rows = await db.Sales.AsNoTracking().Include(x => x.Lines).Where(x => x.CompanyId == context.CompanyId && x.Lines.Any(l => l.ProductVariantId == variantId)).OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(ct); return rows.Select(Map).ToList(); }
    public async Task<DashboardSummaryResponse> GetDashboardAsync(Guid warehouseId, CancellationToken ct) { var today = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero); var recent = await db.Sales.AsNoTracking().Include(x => x.Lines).Where(x => x.CompanyId == context.CompanyId && x.WarehouseId == warehouseId && x.Status == SaleStatus.Confirmed).OrderByDescending(x => x.CreatedAt).Take(5).ToListAsync(ct); var daily = await db.Sales.AsNoTracking().Where(x => x.CompanyId == context.CompanyId && x.WarehouseId == warehouseId && x.Status == SaleStatus.Confirmed && x.CreatedAt >= today).ToListAsync(ct); var stock = await inventoryReport.GetSummaryAsync(context.CompanyId ?? Guid.Empty, warehouseId, 5, ct); return new(daily.Sum(x => x.Total), daily.Count, stock.TotalCost, stock.LowStockProducts, recent.Select(Map).ToList()); }
    private static Result<SaleResponse> Fail(string code, string message) => Result<SaleResponse>.Failure(new(code, message));
    private static SaleResponse Map(Sale x) => new(x.Id, x.WarehouseId, x.Number, x.CustomerIdentification, x.CustomerName, x.CustomerEmail, x.IsFinalConsumer, x.PaymentMethod, x.Status.ToString(), x.Total, x.CreatedAt, x.CreatedBy, x.Lines.Select(l => new SaleLineResponse(l.ProductVariantId, l.ProductName, l.Sku, l.Quantity, l.UnitPrice, l.Quantity * l.UnitPrice)).ToList());
}
public static class SalesModule { public static IServiceCollection AddSalesModule(this IServiceCollection s, IConfiguration c) { s.AddDbContext<SalesDbContext>(o => o.UseNpgsql(c.GetConnectionString("Database"))); s.AddScoped<ISalesService, SalesService>(); return s; } }
