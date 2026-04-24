using EventRegistration.SharedKernel.Domain;

namespace EventRegistration.Events.Domain;

/// <summary>
/// イベント情報を表す集約ルート。
/// </summary>
public sealed class Event : Entity<Guid>
{
    /// <summary>イベント名。</summary>
    public string Name { get; private set; }

    /// <summary>イベントの説明。</summary>
    public string? Description { get; private set; }

    /// <summary>開催日時。</summary>
    public DateTimeOffset ScheduledAt { get; private set; }

    /// <summary>定員（1 以上）。</summary>
    public int Capacity { get; private set; }

    /// <summary>作成日時。</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>EF Core マテリアライゼーション用。</summary>
    private Event() : base()
    {
        Name = string.Empty;
    }

    private Event(Guid id, string name, string? description, DateTimeOffset scheduledAt, int capacity, DateTimeOffset createdAt)
        : base(id)
    {
        Name = name;
        Description = description;
        ScheduledAt = scheduledAt;
        Capacity = capacity;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// 新しいイベントを作成する。
    /// </summary>
    /// <param name="name">イベント名（必須）。</param>
    /// <param name="description">イベントの説明（任意）。</param>
    /// <param name="scheduledAt">開催日時（必須、デフォルト値不可）。</param>
    /// <param name="capacity">定員（1 以上）。</param>
    /// <param name="createdAt">作成日時（必須、デフォルト値不可）。</param>
    /// <returns>新しい <see cref="Event"/> インスタンス。</returns>
    public static Event Create(string name, string? description, DateTimeOffset scheduledAt, int capacity, DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);

        if (scheduledAt == default)
        {
            throw new ArgumentException("開催日時を指定してください。", nameof(scheduledAt));
        }

        if (createdAt == default)
        {
            throw new ArgumentException("作成日時を指定してください。", nameof(createdAt));
        }

        return new Event(Guid.NewGuid(), name, description, scheduledAt, capacity, createdAt);
    }
}
