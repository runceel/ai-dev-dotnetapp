using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Domain;

namespace EventRegistration.Events.Application.UseCases;

/// <summary>
/// すべてのイベントを取得するユースケース。
/// </summary>
public sealed class GetAllEventsUseCase(IEventRepository eventRepository)
{
    public async Task<IReadOnlyList<Event>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return await eventRepository.GetAllAsync(cancellationToken);
    }
}
