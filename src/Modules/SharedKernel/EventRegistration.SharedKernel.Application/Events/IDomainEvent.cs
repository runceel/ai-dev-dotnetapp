namespace EventRegistration.SharedKernel.Application.Events;

/// <summary>
/// ドメインイベントを表すマーカーインターフェース。
/// モジュール間で疎結合に通知を行うための共通契約。
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// イベントが発生した日時。
    /// </summary>
    DateTimeOffset OccurredAt { get; }
}
