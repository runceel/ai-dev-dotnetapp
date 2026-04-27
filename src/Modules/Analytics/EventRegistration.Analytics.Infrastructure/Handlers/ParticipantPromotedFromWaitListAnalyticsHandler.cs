using EventRegistration.Analytics.Application.Repositories;
using EventRegistration.Analytics.Domain;
using EventRegistration.SharedKernel.Application.Events;

namespace EventRegistration.Analytics.Infrastructure.Handlers;

/// <summary>
/// <see cref="ParticipantPromotedFromWaitListEvent"/> を購読し、Analytics のアクティビティとして記録するハンドラ。
/// </summary>
public sealed class ParticipantPromotedFromWaitListAnalyticsHandler(IRegistrationActivityRepository repository)
    : IDomainEventHandler<ParticipantPromotedFromWaitListEvent>
{
    public async Task HandleAsync(ParticipantPromotedFromWaitListEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var activity = RegistrationActivity.Create(
            eventId: domainEvent.EventId,
            registrationId: domainEvent.RegistrationId,
            activityType: RegistrationActivityType.PromotedFromWaitList,
            occurredAt: domainEvent.OccurredAt);

        await repository.AddAsync(activity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
