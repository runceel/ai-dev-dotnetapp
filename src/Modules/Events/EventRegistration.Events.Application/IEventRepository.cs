namespace EventRegistration.Events.Application;

/// <summary>
/// イベントの永続化を担うリポジトリインターフェース。
/// </summary>
public interface IEventRepository
{
    /// <summary>全イベントを開催日時の降順で取得する。</summary>
    Task<IReadOnlyList<Domain.Event>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>指定 ID のイベントを取得する。見つからない場合は null。</summary>
    Task<Domain.Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>イベントを永続化する。</summary>
    Task AddAsync(Domain.Event @event, CancellationToken cancellationToken = default);
}
