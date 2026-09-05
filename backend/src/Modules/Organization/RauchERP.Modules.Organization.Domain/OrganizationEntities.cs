using RauchERP.SharedKernel;

namespace RauchERP.Modules.Organization.Domain;

public sealed class Company : AggregateRoot
{
    private Company() { }
    public Company(string legalName, string tradeName, string taxId, string? digit, string email, string phone, string address, string city, string country, string currency, string timeZone, DateTimeOffset now)
    { if (string.IsNullOrWhiteSpace(legalName) || string.IsNullOrWhiteSpace(taxId)) throw new DomainException("Legal name and tax identification are required."); Id = Guid.NewGuid(); LegalName = legalName.Trim(); TradeName = tradeName.Trim(); TaxIdentification = taxId.Trim(); VerificationDigit = digit; Email = email.Trim(); Phone = phone.Trim(); Address = address.Trim(); City = city.Trim(); CountryCode = country.Trim().ToUpperInvariant(); CurrencyCode = currency.Trim().ToUpperInvariant(); TimeZoneId = timeZone; IsActive = true; CreatedAt = UpdatedAt = now; }
    public string LegalName { get; private set; } = string.Empty; public string TradeName { get; private set; } = string.Empty; public string TaxIdentification { get; private set; } = string.Empty; public string? VerificationDigit { get; private set; } public string Email { get; private set; } = string.Empty; public string Phone { get; private set; } = string.Empty; public string Address { get; private set; } = string.Empty; public string City { get; private set; } = string.Empty; public string CountryCode { get; private set; } = string.Empty; public string CurrencyCode { get; private set; } = string.Empty; public string TimeZoneId { get; private set; } = string.Empty; public bool IsActive { get; private set; } public DateTimeOffset CreatedAt { get; private set; } public DateTimeOffset UpdatedAt { get; private set; }
}
public sealed class Branch : AggregateRoot
{
    private Branch() { }
    public Branch(Guid companyId, string code, string name, string address, string city, string phone, DateTimeOffset now) { Id = Guid.NewGuid(); CompanyId = companyId; Code = code.Trim().ToUpperInvariant(); Name = name.Trim(); Address = address.Trim(); City = city.Trim(); Phone = phone.Trim(); IsActive = true; CreatedAt = UpdatedAt = now; }
    public Guid CompanyId { get; private set; } public string Code { get; private set; } = string.Empty; public string Name { get; private set; } = string.Empty; public string Address { get; private set; } = string.Empty; public string City { get; private set; } = string.Empty; public string Phone { get; private set; } = string.Empty; public bool IsActive { get; private set; } public DateTimeOffset CreatedAt { get; private set; } public DateTimeOffset UpdatedAt { get; private set; }
    public void SetActive(bool active, DateTimeOffset now) { IsActive = active; UpdatedAt = now; }
}
public sealed class Warehouse : AggregateRoot
{
    private Warehouse() { }
    public Warehouse(Guid companyId, Guid branchId, string code, string name, bool isDefault, DateTimeOffset now) { Id = Guid.NewGuid(); CompanyId = companyId; BranchId = branchId; Code = code.Trim().ToUpperInvariant(); Name = name.Trim(); IsDefault = isDefault; IsActive = true; CreatedAt = UpdatedAt = now; }
    public Guid CompanyId { get; private set; } public Guid BranchId { get; private set; } public string Code { get; private set; } = string.Empty; public string Name { get; private set; } = string.Empty; public bool IsDefault { get; private set; } public bool IsActive { get; private set; } public DateTimeOffset CreatedAt { get; private set; } public DateTimeOffset UpdatedAt { get; private set; }
    public void SetActive(bool active, DateTimeOffset now) { IsActive = active; UpdatedAt = now; }
}
public sealed class UserCompanyAccess { public Guid UserId { get; init; } public Guid CompanyId { get; init; } }
public sealed class UserBranchAccess { public Guid UserId { get; init; } public Guid BranchId { get; init; } }
