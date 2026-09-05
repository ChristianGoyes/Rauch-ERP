using RauchERP.Modules.Sales.Contracts;
using RauchERP.SharedKernel;
namespace RauchERP.Modules.Sales.Application;
public interface ISalesService
{
    Task<Result<SaleResponse>> CreateAsync(CreateSaleRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<SaleResponse>> GetAsync(Guid warehouseId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SaleResponse>> GetByVariantAsync(Guid variantId, CancellationToken cancellationToken);
    Task<DashboardSummaryResponse> GetDashboardAsync(Guid warehouseId, CancellationToken cancellationToken);
}
