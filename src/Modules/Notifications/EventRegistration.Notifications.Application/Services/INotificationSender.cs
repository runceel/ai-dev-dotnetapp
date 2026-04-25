using EventRegistration.Notifications.Domain;

namespace EventRegistration.Notifications.Application.Services;

/// <summary>
/// 通知の送信を抽象化するインターフェース。
/// 既定実装はログ出力のみ。本番では SMTP / 外部メール API を用いた実装に差し替える想定。
/// </summary>
public interface INotificationSender
{
    Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default);
}
