using EventRegistration.Events.Domain;

namespace EventRegistration.Events.Application.Repositories;

/// <summary>
/// イベントリポジトリのインターフェース。
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// 指定した ID のイベントを取得する。
    /// </summary>
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// すべてのイベントを取得する。
    /// </summary>
    Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// イベントを追加し、永続化する。
    /// </summary>
    Task AddAsync(Event @event, CancellationToken cancellationToken = default);
}
