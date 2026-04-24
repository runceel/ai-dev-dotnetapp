namespace EventRegistration.SharedKernel.Domain;

/// <summary>
/// すべてのエンティティの基底クラス。ID による等価性を提供する。
/// </summary>
public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected init; } = default!;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (EqualityComparer<TId>.Default.Equals(Id, default!) ||
            EqualityComparer<TId>.Default.Equals(other.Id, default!))
            return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode() =>
        EqualityComparer<TId>.Default.Equals(Id, default!) ? 0 : Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
        !Equals(left, right);
}
