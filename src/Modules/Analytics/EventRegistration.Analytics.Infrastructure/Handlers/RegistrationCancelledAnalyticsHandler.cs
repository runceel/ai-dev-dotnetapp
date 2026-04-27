using EventRegistration.Analytics.Application.Repositories;
using EventRegistration.Analytics.Domain;
using EventRegistration.SharedKernel.Application.Events;

namespace EventRegistration.Analytics.Infrastructure.Handlers;

/// <summary>
/// <see cref="RegistrationCancelledEvent"/> を購読し、Analytics のアクティビティとして記録するハンドラ。
/// </summary>
public sealed class RegistrationCancelledAnalyticsHandler(IRegistrationActivityRepository repository)
    : IDomainEventHandler<RegistrationCancelledEvent>
{
    public async Task HandleAsync(RegistrationCancelledEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var activity = RegistrationActivity.Create(
            eventId: domainEvent.EventId,
            registrationId: domainEvent.RegistrationId,
            activityType: RegistrationActivityType.Cancelled,
            occurredAt: domainEvent.OccurredAt);

        await repository.AddAsync(activity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
