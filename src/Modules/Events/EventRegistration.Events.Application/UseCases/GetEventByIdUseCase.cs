using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Domain;

namespace EventRegistration.Events.Application.UseCases;

/// <summary>
/// イベントを ID で取得するユースケース。
/// </summary>
public sealed class GetEventByIdUseCase(IEventRepository eventRepository)
{
    public async Task<Event?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await eventRepository.GetByIdAsync(id, cancellationToken);
    }
}
