using RauchERP.Modules.Catalog.Domain;
using RauchERP.Modules.Identity.Domain;
using RauchERP.Modules.Inventory.Domain;

namespace RauchERP.UnitTests;

public sealed class CatalogDomainTests
{
    private static ProductVariant ValidVariant() => ProductVariant.Create(Guid.NewGuid(), "RING-6", null, "Talla 6", "Unidad", 10m, 20m, 19m, DateTimeOffset.UtcNow).Value!;
    [Fact] public void Product_with_variant_is_valid() => Assert.True(Product.Create(Guid.NewGuid(), Guid.NewGuid(), null, "Anillo", null, "Stock", true, [ValidVariant()], DateTimeOffset.UtcNow).IsSuccess);
    [Fact] public void Product_without_variants_is_rejected() => Assert.Equal("catalog.product_variant_required", Product.Create(Guid.NewGuid(), Guid.NewGuid(), null, "Anillo", null, "Stock", true, [], DateTimeOffset.UtcNow).Error.Code);
    [Fact] public void Negative_price_is_rejected() => Assert.Equal("catalog.price_negative", ProductVariant.Create(Guid.NewGuid(), "A", null, "A", "Unidad", 0, -1, 0, DateTimeOffset.UtcNow).Error.Code);
    [Fact] public void Negative_cost_is_rejected() => Assert.Equal("catalog.cost_negative", ProductVariant.Create(Guid.NewGuid(), "A", null, "A", "Unidad", -1, 0, 0, DateTimeOffset.UtcNow).Error.Code);
    [Fact] public void Category_requires_name() => Assert.True(Category.Create(Guid.NewGuid(), "", null, null, DateTimeOffset.UtcNow).IsFailure);
    [Fact] public void Product_can_be_deactivated_and_reactivated() { var product = Product.Create(Guid.NewGuid(), Guid.NewGuid(), null, "Anillo", null, "Stock", true, [ValidVariant()], DateTimeOffset.UtcNow).Value!; product.SetActive(false, DateTimeOffset.UtcNow); Assert.False(product.IsActive); product.SetActive(true, DateTimeOffset.UtcNow); Assert.True(product.IsActive); }
}

public sealed class InventoryDomainTests
{
    [Fact] public void Initial_stock_and_positive_adjustment_increase_balance() { var b = New(); Assert.True(b.Apply(StockMovementType.InitialStock, 5, 10, DateTimeOffset.UtcNow).IsSuccess); Assert.True(b.Apply(StockMovementType.PositiveAdjustment, 2, 10, DateTimeOffset.UtcNow).IsSuccess); Assert.Equal(7, b.QuantityOnHand); }
    [Fact] public void Negative_adjustment_decreases_balance() { var b = New(); b.Apply(StockMovementType.InitialStock, 5, 10, DateTimeOffset.UtcNow); Assert.True(b.Apply(StockMovementType.NegativeAdjustment, 2, 10, DateTimeOffset.UtcNow).IsSuccess); Assert.Equal(3, b.QuantityOnHand); }
    [Fact] public void Purchase_uses_weighted_average_cost() { var b = New(); b.Apply(StockMovementType.InitialStock, 10, 100, DateTimeOffset.UtcNow); b.Apply(StockMovementType.PurchaseReceipt, 5, 160, DateTimeOffset.UtcNow); Assert.Equal(120, b.AverageUnitCost); }
    [Fact] public void Sale_keeps_current_average_cost() { var b = New(); b.Apply(StockMovementType.InitialStock, 10, 100, DateTimeOffset.UtcNow); b.Apply(StockMovementType.Sale, 2, 100, DateTimeOffset.UtcNow); Assert.Equal(100, b.AverageUnitCost); }
    [Fact] public void Negative_stock_is_rejected() { var b = New(); var result = b.Apply(StockMovementType.NegativeAdjustment, 1, 0, DateTimeOffset.UtcNow); Assert.Equal("inventory.negative_stock", result.Error.Code); Assert.Equal(0, b.QuantityOnHand); }
    private static StockBalance New() => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
}

public sealed class SessionTests
{
    [Fact] public void Revoked_session_is_invalid() { var now = DateTimeOffset.UtcNow; var session = new UserSession(Guid.NewGuid(), "hash", now, now.AddDays(1), null); session.Revoke(now); Assert.False(session.IsValid(now)); }
}
