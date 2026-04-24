using System.ComponentModel.DataAnnotations;

namespace EventRegistration.Events.Application;

/// <summary>
/// イベント作成時の入力モデル。
/// </summary>
public sealed class CreateEventInput
{
    [Required(ErrorMessage = "イベント名は必須です。")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "開催日時は必須です。")]
    public DateTimeOffset? ScheduledAt { get; set; }

    [Required(ErrorMessage = "定員は必須です。")]
    [Range(1, int.MaxValue, ErrorMessage = "定員は 1 以上である必要があります。")]
    public int? Capacity { get; set; }
}
