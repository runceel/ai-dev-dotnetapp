namespace EventRegistration.Events.Application;

/// <summary>
/// Events モジュールのアプリケーションサービスインターフェース。
/// </summary>
public interface IEventsAppService
{
    /// <summary>全イベントを開催日時の降順で取得する。</summary>
    Task<IReadOnlyList<EventDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>指定 ID のイベントを取得する。見つからない場合は null。</summary>
    Task<EventDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>新しいイベントを作成し、作成されたイベントの DTO を返す。</summary>
    Task<EventDto> CreateAsync(CreateEventInput input, CancellationToken cancellationToken = default);
}
