using EventRegistration.Analytics.Application.Repositories;
using EventRegistration.Analytics.Domain;
using EventRegistration.SharedKernel.Application.Events;

namespace EventRegistration.Analytics.Infrastructure.Handlers;

/// <summary>
/// <see cref="ParticipantWaitListedEvent"/> を購読し、Analytics のアクティビティとして記録するハンドラ。
/// </summary>
public sealed class ParticipantWaitListedAnalyticsHandler(IRegistrationActivityRepository repository)
    : IDomainEventHandler<ParticipantWaitListedEvent>
{
    public async Task HandleAsync(ParticipantWaitListedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var activity = RegistrationActivity.Create(
            eventId: domainEvent.EventId,
            registrationId: domainEvent.RegistrationId,
            activityType: RegistrationActivityType.WaitListed,
            occurredAt: domainEvent.OccurredAt);

        await repository.AddAsync(activity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
