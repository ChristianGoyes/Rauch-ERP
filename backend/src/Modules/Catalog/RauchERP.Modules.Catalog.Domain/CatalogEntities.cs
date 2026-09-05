using RauchERP.SharedKernel;

namespace RauchERP.Modules.Catalog.Domain;

public sealed class Category : AggregateRoot
{
    private Category() { }
    private Category(Guid companyId, string name, string? description, Guid? parentId, DateTimeOffset now) { Id = Guid.NewGuid(); CompanyId = companyId; Name = name.Trim(); Description = description?.Trim(); ParentCategoryId = parentId; IsActive = true; CreatedAt = UpdatedAt = now; }
    public Guid CompanyId { get; private set; } public string Name { get; private set; } = string.Empty; public string? Description { get; private set; } public Guid? ParentCategoryId { get; private set; } public bool IsActive { get; private set; } public DateTimeOffset CreatedAt { get; private set; } public DateTimeOffset UpdatedAt { get; private set; }
    public static Result<Category> Create(Guid companyId, string name, string? description, Guid? parentId, DateTimeOffset now) => string.IsNullOrWhiteSpace(name) ? Result<Category>.Failure(new("catalog.category_name_required", "Category name is required.")) : Result<Category>.Success(new(companyId, name, description, parentId, now));
}
public sealed class Brand : AggregateRoot
{
    private Brand() { }
    public Brand(Guid companyId, string name, string? description) { Id = Guid.NewGuid(); CompanyId = companyId; Name = name.Trim(); Description = description?.Trim(); IsActive = true; }
    public Guid CompanyId { get; private set; } public string Name { get; private set; } = string.Empty; public string? Description { get; private set; } public bool IsActive { get; private set; }
}
public sealed class Product : AggregateRoot
{
    private readonly List<ProductVariant> _variants = [];
    private Product() { }
    private Product(Guid companyId, Guid categoryId, Guid? brandId, string name, string? description, string type, bool tracksInventory, DateTimeOffset now) { Id = Guid.NewGuid(); CompanyId = companyId; CategoryId = categoryId; BrandId = brandId; Name = name.Trim(); Description = description?.Trim(); ProductType = type.Trim(); TracksInventory = tracksInventory; IsActive = true; CreatedAt = UpdatedAt = now; }
    public Guid CompanyId { get; private set; } public Guid CategoryId { get; private set; } public Guid? BrandId { get; private set; } public string Name { get; private set; } = string.Empty; public string? Description { get; private set; } public string ProductType { get; private set; } = string.Empty; public bool TracksInventory { get; private set; } public bool IsActive { get; private set; } public DateTimeOffset CreatedAt { get; private set; } public DateTimeOffset UpdatedAt { get; private set; } public IReadOnlyCollection<ProductVariant> Variants => _variants;
    public static Result<Product> Create(Guid companyId, Guid categoryId, Guid? brandId, string name, string? description, string type, bool tracksInventory, IEnumerable<ProductVariant> variants, DateTimeOffset now)
    { var list = variants.ToList(); if (string.IsNullOrWhiteSpace(name)) return Result<Product>.Failure(new("catalog.product_name_required", "Product name is required.")); if (list.Count == 0) return Result<Product>.Failure(new("catalog.product_variant_required", "A product must have at least one variant.")); var p = new Product(companyId, categoryId, brandId, name, description, type, tracksInventory, now); foreach (var v in list) { v.SetProduct(p.Id); p._variants.Add(v); } return Result<Product>.Success(p); }
    public Result AddVariant(ProductVariant variant, DateTimeOffset now) { if (!IsActive && variant.IsActive) return Result.Failure(new("catalog.product_inactive", "Reactivate the product before adding an active variant.")); _variants.Add(variant); UpdatedAt = now; return Result.Success(); }
    public Result Update(Guid categoryId, Guid? brandId, string name, string? description, string type, bool tracksInventory, DateTimeOffset now) { if (string.IsNullOrWhiteSpace(name)) return Result.Failure(new("catalog.product_name_required", "Product name is required.")); CategoryId = categoryId; BrandId = brandId; Name = name.Trim(); Description = description?.Trim(); ProductType = type.Trim(); TracksInventory = tracksInventory; UpdatedAt = now; return Result.Success(); }
    public void SetActive(bool active, DateTimeOffset now) { IsActive = active; UpdatedAt = now; }
}
public sealed class ProductVariant : Entity
{
    private readonly List<ProductVariantAttribute> _attributes = [];
    private ProductVariant() { }
    private ProductVariant(Guid companyId, string sku, string? barcode, string name, string unitOfMeasure, decimal cost, decimal price, decimal taxRate, DateTimeOffset now) { Id = Guid.NewGuid(); CompanyId = companyId; Sku = sku.Trim().ToUpperInvariant(); Barcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim(); Name = name.Trim(); UnitOfMeasure = unitOfMeasure.Trim(); Cost = cost; BasePrice = price; TaxRate = taxRate; IsActive = true; CreatedAt = UpdatedAt = now; }
    public Guid CompanyId { get; private set; } public Guid ProductId { get; private set; } public string Sku { get; private set; } = string.Empty; public string? Barcode { get; private set; } public string Name { get; private set; } = string.Empty; public string UnitOfMeasure { get; private set; } = "Unidad"; public decimal Cost { get; private set; } public decimal BasePrice { get; private set; } public decimal TaxRate { get; private set; } public bool IsActive { get; private set; } public DateTimeOffset CreatedAt { get; private set; } public DateTimeOffset UpdatedAt { get; private set; } public IReadOnlyCollection<ProductVariantAttribute> Attributes => _attributes;
    public static Result<ProductVariant> Create(Guid companyId, string sku, string? barcode, string name, string unitOfMeasure, decimal cost, decimal price, decimal taxRate, DateTimeOffset now)
    { if (string.IsNullOrWhiteSpace(sku)) return Result<ProductVariant>.Failure(new("catalog.sku_required", "SKU is required.")); if (string.IsNullOrWhiteSpace(unitOfMeasure)) return Result<ProductVariant>.Failure(new("catalog.unit_required", "Unit of measure is required.")); if (cost < 0) return Result<ProductVariant>.Failure(new("catalog.cost_negative", "Cost cannot be negative.")); if (price < 0) return Result<ProductVariant>.Failure(new("catalog.price_negative", "Price cannot be negative.")); if (taxRate is < 0 or > 100) return Result<ProductVariant>.Failure(new("catalog.tax_rate_invalid", "Tax rate must be between 0 and 100.")); return Result<ProductVariant>.Success(new(companyId, sku, barcode, name, unitOfMeasure, cost, price, taxRate, now)); }
    public void SetProduct(Guid productId) => ProductId = productId;
    public Result Update(string sku, string? barcode, string name, string unitOfMeasure, decimal cost, decimal salePrice, decimal taxRate, DateTimeOffset now) { if (string.IsNullOrWhiteSpace(sku)) return Result.Failure(new("catalog.sku_required", "SKU is required.")); if (string.IsNullOrWhiteSpace(unitOfMeasure)) return Result.Failure(new("catalog.unit_required", "Unit of measure is required.")); if (cost < 0 || salePrice < 0) return Result.Failure(new("catalog.price_invalid", "Cost and sale price cannot be negative.")); if (taxRate is < 0 or > 100) return Result.Failure(new("catalog.tax_rate_invalid", "Tax rate must be between 0 and 100.")); Sku = sku.Trim().ToUpperInvariant(); Barcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim(); Name = name.Trim(); UnitOfMeasure = unitOfMeasure.Trim(); Cost = cost; BasePrice = salePrice; TaxRate = taxRate; UpdatedAt = now; return Result.Success(); }
    public void SetActive(bool active, DateTimeOffset now) { IsActive = active; UpdatedAt = now; }
}
public sealed class AttributeDefinition : Entity { private AttributeDefinition() { } public AttributeDefinition(Guid companyId, string name) { Id = Guid.NewGuid(); CompanyId = companyId; Name = name.Trim(); } public Guid CompanyId { get; private set; } public string Name { get; private set; } = string.Empty; }
public sealed class AttributeValue : Entity { private AttributeValue() { } public AttributeValue(Guid definitionId, string value) { Id = Guid.NewGuid(); AttributeDefinitionId = definitionId; Value = value.Trim(); } public Guid AttributeDefinitionId { get; private set; } public string Value { get; private set; } = string.Empty; }
public sealed class ProductVariantAttribute { public Guid ProductVariantId { get; init; } public Guid AttributeValueId { get; init; } }
public sealed class ProductImage : Entity { private ProductImage() { } public ProductImage(Guid productId, string url, int sortOrder) { Id = Guid.NewGuid(); ProductId = productId; Url = url; SortOrder = sortOrder; } public Guid ProductId { get; private set; } public string Url { get; private set; } = string.Empty; public int SortOrder { get; private set; } }
public sealed class PriceList : Entity { public Guid CompanyId { get; init; } public string Name { get; init; } = string.Empty; }
public sealed class ProductPrice : Entity { public Guid PriceListId { get; init; } public Guid ProductVariantId { get; init; } public decimal Price { get; init; } }
