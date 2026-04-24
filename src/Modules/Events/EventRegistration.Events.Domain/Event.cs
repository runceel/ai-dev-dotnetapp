namespace EventRegistration.Events.Domain;

/// <summary>
/// イベント情報を表す集約ルート。
/// </summary>
public class Event
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTimeOffset ScheduledAt { get; private set; }
    public int Capacity { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Event() { }

    public static Event Create(string name, string? description, DateTimeOffset scheduledAt, int capacity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);

        return new Event
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            ScheduledAt = scheduledAt,
            Capacity = capacity,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }
}
