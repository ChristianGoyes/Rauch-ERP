namespace RauchERP.Modules.Catalog.Contracts;

public sealed record CreateCategoryRequest(Guid CompanyId, string Name, string? Description, Guid? ParentCategoryId);
public sealed record CategoryResponse(Guid Id, Guid CompanyId, string Name, string? Description, Guid? ParentCategoryId, bool IsActive);
public sealed record CreateBrandRequest(Guid CompanyId, string Name, string? Description);
public sealed record BrandResponse(Guid Id, Guid CompanyId, string Name, bool IsActive);
public sealed record VariantRequest(string Sku, string? Barcode, string Name, string UnitOfMeasure, decimal Cost, decimal BasePrice, decimal TaxRate, IReadOnlyDictionary<string, string>? Attributes);
public sealed record CreateProductRequest(Guid CompanyId, Guid CategoryId, Guid? BrandId, string Name, string? Description, string ProductType, bool TracksInventory, IReadOnlyCollection<VariantRequest> Variants);
public sealed record UpdateProductRequest(Guid CategoryId, Guid? BrandId, string Name, string? Description, string ProductType, bool TracksInventory);
public sealed record UpdateProductVariantRequest(string Sku, string? Barcode, string Name, string UnitOfMeasure, decimal Cost, decimal SalePrice, decimal TaxRate);
public sealed record ProductResponse(Guid Id, Guid CompanyId, Guid CategoryId, Guid? BrandId, string Name, string? Description, string ProductType, bool TracksInventory, bool IsActive, IReadOnlyCollection<string> ImageUrls, IReadOnlyCollection<ProductVariantResponse> Variants);
public sealed record ProductVariantResponse(Guid Id, string Sku, string? Barcode, string Name, string UnitOfMeasure, decimal Cost, decimal BasePrice, decimal TaxRate, bool IsActive, IReadOnlyDictionary<string, string> Attributes);
public sealed record ProductSearch(Guid CompanyId, Guid? CategoryId, Guid? BrandId, bool? IsActive, bool? TracksInventory, string? Text, int Page = 1, int PageSize = 20, string Sort = "name");
public sealed record ProductVariantInventoryInfo(Guid Id, Guid CompanyId, bool ProductIsActive, bool VariantIsActive, bool TracksInventory);
public interface IProductVariantInventoryView { Task<ProductVariantInventoryInfo?> GetAsync(Guid id, CancellationToken cancellationToken); }
public sealed record ProductVariantLookup(Guid Id, Guid ProductId, string ProductName, string VariantName, string Sku, string? Barcode, string UnitOfMeasure, decimal Cost, decimal SalePrice, bool TracksInventory, bool IsActive);
public interface IProductCatalogView { Task<IReadOnlyList<ProductVariantLookup>> GetVariantsAsync(Guid companyId, CancellationToken cancellationToken); }
public interface IProductVariantPricing { Task<bool> UpdatePricingAsync(Guid variantId, decimal cost, decimal? salePrice, CancellationToken cancellationToken); }
