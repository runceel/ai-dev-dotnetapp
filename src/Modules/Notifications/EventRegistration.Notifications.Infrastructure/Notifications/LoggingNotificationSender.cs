using EventRegistration.Notifications.Application.Services;
using EventRegistration.Notifications.Domain;
using Microsoft.Extensions.Logging;

namespace EventRegistration.Notifications.Infrastructure.Notifications;

/// <summary>
/// 通知を <see cref="ILogger{TCategoryName}"/> 経由で構造化ログとして出力する既定実装。
/// </summary>
/// <remarks>
/// 構造化ログのフィールドとして <c>Kind</c>, <c>EventId</c>, <c>RegistrationId</c>,
/// <c>ParticipantName</c>, <c>ParticipantEmail</c> を出力する。
/// 本番では SMTP / 外部メール API を使う実装に差し替えることを想定。
/// </remarks>
public sealed partial class LoggingNotificationSender(ILogger<LoggingNotificationSender> logger)
    : INotificationSender
{
    public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        LogNotification(
            logger,
            message.Kind,
            message.EventId,
            message.RegistrationId,
            message.ParticipantName,
            message.ParticipantEmail);

        return Task.CompletedTask;
    }

    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Notification dispatched. Kind={Kind}, EventId={EventId}, RegistrationId={RegistrationId}, ParticipantName={ParticipantName}, ParticipantEmail={ParticipantEmail}")]
    private static partial void LogNotification(
        ILogger logger,
        NotificationKind kind,
        Guid eventId,
        Guid registrationId,
        string participantName,
        string participantEmail);
}
