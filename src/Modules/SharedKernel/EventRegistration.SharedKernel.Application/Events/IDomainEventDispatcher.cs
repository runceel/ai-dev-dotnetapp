namespace EventRegistration.SharedKernel.Application.Events;

/// <summary>
/// ドメインイベントを購読しているハンドラ群へ配送するディスパッチャ。
/// </summary>
/// <remarks>
/// 実装は登録されている <see cref="IDomainEventHandler{TEvent}"/> をすべて呼び出すこと。
/// 個々のハンドラで例外が発生してもディスパッチ呼び出し元へ伝播させてはならない
/// （購読側でログに記録するに留める）。これによりユースケースの主処理は副作用の失敗から保護される。
/// </remarks>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// 単一のドメインイベントを配送する。
    /// </summary>
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// 複数のドメインイベントをまとめて配送する。
    /// </summary>
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
