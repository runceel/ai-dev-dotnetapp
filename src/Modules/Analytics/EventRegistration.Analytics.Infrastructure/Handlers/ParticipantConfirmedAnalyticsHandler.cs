using EventRegistration.Analytics.Application.Repositories;
using EventRegistration.Analytics.Domain;
using EventRegistration.SharedKernel.Application.Events;

namespace EventRegistration.Analytics.Infrastructure.Handlers;

/// <summary>
/// <see cref="ParticipantConfirmedEvent"/> を購読し、Analytics のアクティビティとして記録するハンドラ。
/// </summary>
public sealed class ParticipantConfirmedAnalyticsHandler(IRegistrationActivityRepository repository)
    : IDomainEventHandler<ParticipantConfirmedEvent>
{
    public async Task HandleAsync(ParticipantConfirmedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var activity = RegistrationActivity.Create(
            eventId: domainEvent.EventId,
            registrationId: domainEvent.RegistrationId,
            activityType: RegistrationActivityType.Confirmed,
            occurredAt: domainEvent.OccurredAt);

        await repository.AddAsync(activity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
