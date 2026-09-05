using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RauchERP.Modules.Catalog.Application;
using RauchERP.Modules.Catalog.Contracts;
using RauchERP.SharedKernel;

namespace RauchERP.Modules.Catalog.Presentation;
public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var categories = app.MapGroup("/api/v1/categories").WithTags("Categories").RequireAuthorization();
        categories.MapPost("/", async (CreateCategoryRequest r, ICatalogService s, CancellationToken ct) => Http(await s.CreateCategoryAsync(r, ct))).RequireAuthorization("catalog.categories.create");
        categories.MapGet("/", async (Guid companyId, ICatalogService s, CancellationToken ct) => Results.Ok(await s.GetCategoriesAsync(companyId, ct))).RequireAuthorization("catalog.categories.read");
        var brands = app.MapGroup("/api/v1/brands").WithTags("Brands").RequireAuthorization();
        brands.MapPost("/", async (CreateBrandRequest r, ICatalogService s, CancellationToken ct) => Http(await s.CreateBrandAsync(r, ct)));
        brands.MapGet("/", async (Guid companyId, ICatalogService s, CancellationToken ct) => Results.Ok(await s.GetBrandsAsync(companyId, ct)));
        var products = app.MapGroup("/api/v1/products").WithTags("Products").RequireAuthorization();
        products.MapPost("/", async (CreateProductRequest r, ICatalogService s, CancellationToken ct) => Http(await s.CreateProductAsync(r, ct))).RequireAuthorization("catalog.products.create");
        products.MapGet("/{id:guid}", async (Guid id, ICatalogService s, CancellationToken ct) => await s.GetProductAsync(id, ct) is { } p ? Results.Ok(p) : Results.NotFound()).RequireAuthorization("catalog.products.read");
        products.MapGet("/", async (Guid companyId, Guid? categoryId, Guid? brandId, bool? isActive, bool? tracksInventory, string? text, int page, int pageSize, string? sort, ICatalogService s, CancellationToken ct) => Results.Ok(await s.SearchProductsAsync(new(companyId, categoryId, brandId, isActive, tracksInventory, text, page, pageSize, sort ?? "name"), ct))).RequireAuthorization("catalog.products.read");
        products.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest r, ICatalogService s, CancellationToken ct) => Http(await s.UpdateProductAsync(id, r, ct))).RequireAuthorization("catalog.products.update");
        products.MapPut("/variants/{id:guid}", async (Guid id, UpdateProductVariantRequest r, ICatalogService s, CancellationToken ct) => Http(await s.UpdateVariantAsync(id, r, ct))).RequireAuthorization("catalog.products.update");
        products.MapPost("/{id:guid}/images", async (Guid id, IFormFile image, ICatalogService s, CancellationToken ct) => { await using var stream = image.OpenReadStream(); using var memory = new MemoryStream(); await stream.CopyToAsync(memory, ct); return Http(await s.AddProductImageAsync(id, Path.GetExtension(image.FileName), memory.ToArray(), ct)); }).DisableAntiforgery().RequireAuthorization("catalog.products.update");
        products.MapPut("/{id:guid}/active", async (Guid id, bool active, ICatalogService s, CancellationToken ct) => Http(await s.SetProductActiveAsync(id, active, ct))).RequireAuthorization("catalog.products.update");
        products.MapPut("/variants/{id:guid}/active", async (Guid id, bool active, ICatalogService s, CancellationToken ct) => Http(await s.SetVariantActiveAsync(id, active, ct))).RequireAuthorization("catalog.products.update");
        return app;
    }
    private static IResult Http(Result r) => r.IsSuccess ? Results.NoContent() : Results.Problem(r.Error.Description, statusCode: 400, title: r.Error.Code);
    private static IResult Http<T>(Result<T> r) => r.IsSuccess ? Results.Ok(r.Value) : Results.Problem(r.Error.Description, statusCode: 400, title: r.Error.Code);
}
