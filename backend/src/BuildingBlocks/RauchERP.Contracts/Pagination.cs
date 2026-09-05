namespace RauchERP.Contracts;

public sealed record Page<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, long TotalCount);
