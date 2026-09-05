using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RauchERP.Modules.Sales.Application;
using RauchERP.Modules.Sales.Contracts;
using RauchERP.SharedKernel;
namespace RauchERP.Modules.Sales.Presentation;
public static class SalesEndpoints
{
    public static IEndpointRouteBuilder MapSalesEndpoints(this IEndpointRouteBuilder app)
    {
        var sales = app.MapGroup("/api/v1/sales").WithTags("Sales").RequireAuthorization();
        sales.MapPost("/", async (CreateSaleRequest r, ISalesService s, CancellationToken ct) => Http(await s.CreateAsync(r, ct))).RequireAuthorization("sales.create");
        sales.MapGet("/", async (Guid warehouseId, ISalesService s, CancellationToken ct) => Results.Ok(await s.GetAsync(warehouseId, ct))).RequireAuthorization("sales.read");
        sales.MapGet("/variant/{id:guid}", async (Guid id, ISalesService s, CancellationToken ct) => Results.Ok(await s.GetByVariantAsync(id, ct))).RequireAuthorization("sales.read");
        app.MapGet("/api/v1/dashboard/summary", async (Guid warehouseId, ISalesService s, CancellationToken ct) => Results.Ok(await s.GetDashboardAsync(warehouseId, ct))).RequireAuthorization("dashboard.read").WithTags("Dashboard");
        return app;
    }
    private static IResult Http<T>(Result<T> r) => r.IsSuccess ? Results.Ok(r.Value) : Results.Problem(r.Error.Description, statusCode: 400, title: r.Error.Code);
}
