namespace RauchERP.Modules.Organization.Contracts;

public sealed record CreateCompanyRequest(string LegalName, string TradeName, string TaxIdentification, string? VerificationDigit, string Email, string Phone, string Address, string City, string CountryCode, string CurrencyCode, string TimeZoneId);
public sealed record CompanyResponse(Guid Id, string LegalName, string TradeName, string TaxIdentification, bool IsActive);
public sealed record CreateBranchRequest(Guid CompanyId, string Code, string Name, string Address, string City, string Phone);
public sealed record BranchResponse(Guid Id, Guid CompanyId, string Code, string Name, bool IsActive);
public sealed record CreateWarehouseRequest(Guid CompanyId, Guid BranchId, string Code, string Name, bool IsDefault);
public sealed record WarehouseResponse(Guid Id, Guid CompanyId, Guid BranchId, string Code, string Name, bool IsDefault, bool IsActive);
public sealed record AssignAccessRequest(Guid UserId, Guid ResourceId);
public sealed record WarehouseInventoryInfo(Guid Id, Guid CompanyId, Guid BranchId, bool IsActive);
public interface IWarehouseInventoryView { Task<WarehouseInventoryInfo?> GetAsync(Guid id, CancellationToken cancellationToken); }
public interface IOrganizationAccessView { Task<bool> HasAccessAsync(Guid userId, Guid companyId, Guid branchId, CancellationToken cancellationToken); }
