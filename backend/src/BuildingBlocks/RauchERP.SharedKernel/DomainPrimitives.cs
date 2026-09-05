namespace RauchERP.SharedKernel;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}

public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public Guid Id { get; protected init; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

public abstract class AggregateRoot : Entity;

public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();
    public override bool Equals(object? obj) => obj is ValueObject other && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    public override int GetHashCode() => GetEqualityComponents().Aggregate(17, (hash, item) => HashCode.Combine(hash, item));
}

public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}

public class Result
{
    protected Result(bool isSuccess, Error error) { IsSuccess = isSuccess; Error = error; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
}

public sealed class Result<T> : Result
{
    private Result(T? value, bool isSuccess, Error error) : base(isSuccess, error) => Value = value;
    public T? Value { get; }
    public static Result<T> Success(T value) => new(value, true, Error.None);
    public new static Result<T> Failure(Error error) => new(default, false, error);
}

public sealed class DomainException(string message) : Exception(message);
