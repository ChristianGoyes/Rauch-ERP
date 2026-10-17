using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RauchERP.Modules.Organization.Application;
using RauchERP.Modules.Organization.Contracts;
using RauchERP.SharedKernel;

namespace RauchERP.Modules.Organization.Presentation;
public static class OrganizationEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationEndpoints(this IEndpointRouteBuilder app)
    {
        var companies = app.MapGroup("/api/v1/companies").WithTags("Companies").RequireAuthorization();
        companies.MapPost("/", async (CreateCompanyRequest r, IOrganizationService s, CancellationToken ct) => Http(await s.CreateCompanyAsync(r, ct))).RequireAuthorization("organization.companies.create");
        companies.MapGet("/", async (IOrganizationService s, CancellationToken ct) => Results.Ok(await s.GetCompaniesAsync(ct))).RequireAuthorization("organization.companies.read");
        companies.MapPost("/access", async (AssignAccessRequest r, IOrganizationService s, CancellationToken ct) => Http(await s.AssignCompanyAccessAsync(r, ct))).RequireAuthorization("identity.users.update");
        var branches = app.MapGroup("/api/v1/branches").WithTags("Branches").RequireAuthorization();
        branches.MapPost("/", async (CreateBranchRequest r, IOrganizationService s, CancellationToken ct) => Http(await s.CreateBranchAsync(r, ct))).RequireAuthorization("organization.branches.create");
        branches.MapGet("/", async (Guid companyId, IOrganizationService s, CancellationToken ct) => Results.Ok(await s.GetBranchesAsync(companyId, ct))).RequireAuthorization("organization.branches.read");
        branches.MapPut("/{id:guid}/active", async (Guid id, bool active, IOrganizationService s, CancellationToken ct) => Http(await s.SetBranchActiveAsync(id, active, ct))).RequireAuthorization("organization.branches.update");
        branches.MapPost("/access", async (AssignAccessRequest r, IOrganizationService s, CancellationToken ct) => Http(await s.AssignBranchAccessAsync(r, ct))).RequireAuthorization("identity.users.update");
        var warehouses = app.MapGroup("/api/v1/warehouses").WithTags("Warehouses").RequireAuthorization();
        warehouses.MapPost("/", async (CreateWarehouseRequest r, IOrganizationService s, CancellationToken ct) => Http(await s.CreateWarehouseAsync(r, ct))).RequireAuthorization("organization.warehouses.create");
        warehouses.MapGet("/", async (Guid branchId, IOrganizationService s, CancellationToken ct) => Results.Ok(await s.GetWarehousesAsync(branchId, ct))).RequireAuthorization("organization.warehouses.read");
        warehouses.MapPut("/{id:guid}/active", async (Guid id, bool active, IOrganizationService s, CancellationToken ct) => Http(await s.SetWarehouseActiveAsync(id, active, ct))).RequireAuthorization("organization.warehouses.update");
        return app;
    }
    private static IResult Http(Result r) => r.IsSuccess ? Results.NoContent() : Results.Problem(r.Error.Description, statusCode: 400, title: r.Error.Code);
    private static IResult Http<T>(Result<T> r) => r.IsSuccess ? Results.Ok(r.Value) : Results.Problem(r.Error.Description, statusCode: 400, title: r.Error.Code);
}
