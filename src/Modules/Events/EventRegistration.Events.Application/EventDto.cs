namespace EventRegistration.Events.Application;

/// <summary>
/// イベント情報の出力 DTO。
/// </summary>
public sealed record EventDto(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset ScheduledAt,
    int Capacity,
    DateTimeOffset CreatedAt);
