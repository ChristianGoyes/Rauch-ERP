namespace RauchERP.Application.Abstractions;

public interface IClock { DateTimeOffset UtcNow { get; } }
public interface ICurrentUser { Guid? UserId { get; } IReadOnlySet<string> Permissions { get; } }
public interface ICompanyContext { Guid? CompanyId { get; } Guid? BranchId { get; } }
public interface IUnitOfWork { Task<int> SaveChangesAsync(CancellationToken cancellationToken = default); }
public sealed class SystemClock : IClock { public DateTimeOffset UtcNow => DateTimeOffset.UtcNow; }
