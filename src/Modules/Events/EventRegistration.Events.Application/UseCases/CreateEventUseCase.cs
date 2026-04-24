using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Domain;

namespace EventRegistration.Events.Application.UseCases;

/// <summary>
/// 新しいイベントを作成するユースケース。
/// </summary>
public sealed class CreateEventUseCase(IEventRepository eventRepository)
{
    public async Task<Event> ExecuteAsync(
        string name,
        string? description,
        DateTimeOffset scheduledAt,
        int capacity,
        CancellationToken cancellationToken = default)
    {
        var ev = Event.Create(name, description, scheduledAt, capacity);
        await eventRepository.AddAsync(ev, cancellationToken);
        return ev;
    }
}
