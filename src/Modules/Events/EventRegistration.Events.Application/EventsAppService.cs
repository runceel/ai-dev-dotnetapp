namespace EventRegistration.Events.Application;

/// <summary>
/// Events モジュールのアプリケーションサービス実装。
/// </summary>
internal sealed class EventsAppService(IEventRepository repository) : IEventsAppService
{
    public async Task<IReadOnlyList<EventDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var events = await repository.GetAllAsync(cancellationToken);
        return events.Select(ToDto).ToList();
    }

    public async Task<EventDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var @event = await repository.GetByIdAsync(id, cancellationToken);
        return @event is null ? null : ToDto(@event);
    }

    public async Task<EventDto> CreateAsync(CreateEventInput input, CancellationToken cancellationToken = default)
    {
        var @event = Domain.Event.Create(
            input.Name,
            input.Description,
            input.ScheduledAt!.Value,
            input.Capacity!.Value);

        await repository.AddAsync(@event, cancellationToken);
        return ToDto(@event);
    }

    private static EventDto ToDto(Domain.Event e) => new(
        e.Id,
        e.Name,
        e.Description,
        e.ScheduledAt,
        e.Capacity,
        e.CreatedAt);
}
