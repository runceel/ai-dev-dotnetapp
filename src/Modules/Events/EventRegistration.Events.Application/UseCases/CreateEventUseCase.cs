using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Domain;

namespace EventRegistration.Events.Application.UseCases;

/// <summary>
/// 新しいイベントを作成するユースケース。
/// </summary>
public sealed class CreateEventUseCase
{
    private readonly IEventRepository _repository;
    private readonly TimeProvider _timeProvider;

    public CreateEventUseCase(IEventRepository repository, TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// イベントを作成して永続化する。
    /// </summary>
    /// <returns>作成されたイベントの ID。</returns>
    public async Task<Guid> ExecuteAsync(
        string name,
        string? description,
        DateTimeOffset scheduledAt,
        int capacity,
        CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow();
        var @event = Event.Create(name, description, scheduledAt, capacity, now);
        await _repository.AddAsync(@event, cancellationToken);
        return @event.Id;
    }
}
