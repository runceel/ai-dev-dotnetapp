namespace EventRegistration.Events.Domain;

/// <summary>
/// イベント情報を表す集約ルート。
/// </summary>
public sealed class Event : SharedKernel.Domain.Entity<Guid>
{
    /// <summary>イベント名。</summary>
    public string Name { get; private set; } = default!;

    /// <summary>イベントの説明。</summary>
    public string? Description { get; private set; }

    /// <summary>開催日時。</summary>
    public DateTimeOffset ScheduledAt { get; private set; }

    /// <summary>定員（1 以上）。</summary>
    public int Capacity { get; private set; }

    /// <summary>作成日時。</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    // EF Core 用プライベートコンストラクタ
    private Event() { }

    /// <summary>
    /// 新しいイベントを作成する。
    /// </summary>
    public static Event Create(string name, string? description, DateTimeOffset scheduledAt, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("イベント名は必須です。", nameof(name));

        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "定員は 1 以上である必要があります。");

        if (scheduledAt == default)
            throw new ArgumentException("開催日時は必須です。", nameof(scheduledAt));

        return new Event
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            ScheduledAt = scheduledAt,
            Capacity = capacity,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }
}
