using RauchERP.Modules.Catalog.Contracts;
using RauchERP.SharedKernel;

namespace RauchERP.Modules.Catalog.Application;
public interface ICatalogService
{
    Task<Result<CategoryResponse>> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(Guid companyId, CancellationToken cancellationToken);
    Task<Result<BrandResponse>> CreateBrandAsync(CreateBrandRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<BrandResponse>> GetBrandsAsync(Guid companyId, CancellationToken cancellationToken);
    Task<Result<ProductResponse>> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken);
    Task<ProductResponse?> GetProductAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductResponse>> SearchProductsAsync(ProductSearch search, CancellationToken cancellationToken);
    Task<Result<ProductResponse>> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken);
    Task<Result<ProductVariantResponse>> UpdateVariantAsync(Guid id, UpdateProductVariantRequest request, CancellationToken cancellationToken);
    Task<Result<string>> AddProductImageAsync(Guid productId, string extension, byte[] content, CancellationToken cancellationToken);
    Task<Result> SetProductActiveAsync(Guid id, bool active, CancellationToken cancellationToken);
    Task<Result> SetVariantActiveAsync(Guid id, bool active, CancellationToken cancellationToken);
}
