namespace EventRegistration.SharedKernel.Domain;

/// <summary>
/// すべてのエンティティの共通基底クラス。
/// ID による等価性を提供する。
/// </summary>
/// <typeparam name="TId">ID の型。</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull
{
    /// <summary>エンティティの一意識別子。</summary>
    public TId Id { get; protected init; }

    /// <summary>
    /// 指定した ID でエンティティを初期化する。
    /// </summary>
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// EF Core のマテリアライゼーション用パラメーターなしコンストラクター。
    /// </summary>
    protected Entity()
    {
        Id = default!;
    }

    public override bool Equals(object? obj) => obj is Entity<TId> other && Equals(other);

    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // デフォルト ID（未永続化）のエンティティは参照等価のみ
        if (EqualityComparer<TId>.Default.Equals(Id, default!) ||
            EqualityComparer<TId>.Default.Equals(other.Id, default!))
        {
            return false;
        }

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode() => EqualityComparer<TId>.Default.GetHashCode(Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}
