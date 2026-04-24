using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Domain;

namespace EventRegistration.Events.Application.UseCases;

/// <summary>
/// イベント一覧を取得するユースケース。
/// 開催日時の降順でソートして返す。
/// </summary>
public sealed class GetEventsUseCase
{
    private readonly IEventRepository _repository;

    public GetEventsUseCase(IEventRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// すべてのイベントを開催日時の降順で取得する。
    /// </summary>
    public async Task<IReadOnlyList<Event>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var events = await _repository.GetAllAsync(cancellationToken);
        return events.OrderByDescending(e => e.ScheduledAt).ToList().AsReadOnly();
    }
}
