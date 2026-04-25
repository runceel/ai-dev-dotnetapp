namespace EventRegistration.SharedKernel.Application.Events;

/// <summary>
/// 特定のドメインイベントを購読するハンドラ。
/// </summary>
/// <typeparam name="TEvent">処理対象のドメインイベント型。</typeparam>
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
