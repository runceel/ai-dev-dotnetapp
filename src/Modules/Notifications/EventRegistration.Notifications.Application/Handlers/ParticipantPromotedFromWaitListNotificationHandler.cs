using EventRegistration.Notifications.Application.Services;
using EventRegistration.Notifications.Domain;
using EventRegistration.SharedKernel.Application.Events;

namespace EventRegistration.Notifications.Application.Handlers;

/// <summary>
/// キャンセル待ち繰り上げ (<see cref="ParticipantPromotedFromWaitListEvent"/>) を購読し、通知を送信するハンドラ。
/// </summary>
public sealed class ParticipantPromotedFromWaitListNotificationHandler(INotificationSender notificationSender)
    : IDomainEventHandler<ParticipantPromotedFromWaitListEvent>
{
    public Task HandleAsync(ParticipantPromotedFromWaitListEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var message = new NotificationMessage(
            Kind: NotificationKind.ParticipantPromotedFromWaitList,
            EventId: domainEvent.EventId,
            RegistrationId: domainEvent.RegistrationId,
            ParticipantName: domainEvent.ParticipantName,
            ParticipantEmail: domainEvent.ParticipantEmail);

        return notificationSender.SendAsync(message, cancellationToken);
    }
}
