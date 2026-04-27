namespace EventRegistration.Analytics.Domain;

/// <summary>
/// Analytics モジュールが受信したドメインイベント 1 件分のアクティビティを表すルートエンティティ。
/// 自モジュールの Read モデル構築のためにのみ用いる（CQRS の Read 側）。
/// </summary>
public class RegistrationActivity
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Guid RegistrationId { get; private set; }
    public RegistrationActivityType ActivityType { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private RegistrationActivity() { }

    /// <summary>
    /// アクティビティを生成する。
    /// </summary>
    public static RegistrationActivity Create(
        Guid eventId,
        Guid registrationId,
        RegistrationActivityType activityType,
        DateTimeOffset occurredAt)
    {
        return new RegistrationActivity
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            RegistrationId = registrationId,
            ActivityType = activityType,
            OccurredAt = occurredAt,
        };
    }
}
