using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RauchERP.Application.Abstractions;

namespace RauchERP.Infrastructure.Shared;

public sealed class HttpExecutionContext(IHttpContextAccessor accessor) : ICurrentUser, ICompanyContext
{
    private HttpContext? HttpContext => accessor.HttpContext;
    public Guid? UserId => Guid.TryParse(HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public Guid? CompanyId => ReadGuid("X-Company-Id");
    public Guid? BranchId => ReadGuid("X-Branch-Id");
    public IReadOnlySet<string> Permissions => HttpContext?.User.FindAll("permission").Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>();
    private Guid? ReadGuid(string name) => Guid.TryParse(HttpContext?.Request.Headers[name], out var id) ? id : null;
}
