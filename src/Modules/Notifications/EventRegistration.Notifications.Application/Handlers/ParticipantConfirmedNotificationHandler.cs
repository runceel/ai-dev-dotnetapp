using EventRegistration.Notifications.Application.Services;
using EventRegistration.Notifications.Domain;
using EventRegistration.SharedKernel.Application.Events;

namespace EventRegistration.Notifications.Application.Handlers;

/// <summary>
/// 参加確定 (<see cref="ParticipantConfirmedEvent"/>) を購読し、通知を送信するハンドラ。
/// </summary>
public sealed class ParticipantConfirmedNotificationHandler(INotificationSender notificationSender)
    : IDomainEventHandler<ParticipantConfirmedEvent>
{
    public Task HandleAsync(ParticipantConfirmedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var message = new NotificationMessage(
            Kind: NotificationKind.ParticipantConfirmed,
            EventId: domainEvent.EventId,
            RegistrationId: domainEvent.RegistrationId,
            ParticipantName: domainEvent.ParticipantName,
            ParticipantEmail: domainEvent.ParticipantEmail);

        return notificationSender.SendAsync(message, cancellationToken);
    }
}
