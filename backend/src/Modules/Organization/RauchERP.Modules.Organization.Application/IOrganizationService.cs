using RauchERP.Modules.Organization.Contracts;
using RauchERP.SharedKernel;

namespace RauchERP.Modules.Organization.Application;
public interface IOrganizationService
{
    Task<Result<CompanyResponse>> CreateCompanyAsync(CreateCompanyRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<CompanyResponse>> GetCompaniesAsync(CancellationToken cancellationToken);
    Task<Result<BranchResponse>> CreateBranchAsync(CreateBranchRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<BranchResponse>> GetBranchesAsync(Guid companyId, CancellationToken cancellationToken);
    Task<Result<WarehouseResponse>> CreateWarehouseAsync(CreateWarehouseRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<WarehouseResponse>> GetWarehousesAsync(Guid branchId, CancellationToken cancellationToken);
    Task<Result> SetBranchActiveAsync(Guid id, bool active, CancellationToken cancellationToken);
    Task<Result> SetWarehouseActiveAsync(Guid id, bool active, CancellationToken cancellationToken);
    Task<Result> AssignCompanyAccessAsync(AssignAccessRequest request, CancellationToken cancellationToken);
    Task<Result> AssignBranchAccessAsync(AssignAccessRequest request, CancellationToken cancellationToken);
}
