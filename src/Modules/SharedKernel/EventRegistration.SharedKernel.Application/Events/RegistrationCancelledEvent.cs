namespace EventRegistration.SharedKernel.Application.Events;

/// <summary>
/// <see cref="RegistrationCancelledEvent"/> の発行時に、キャンセル直前の登録ステータスを表す列挙体。
/// </summary>
/// <remarks>
/// SharedKernel から Registrations モジュール固有の <c>RegistrationStatus</c> を参照しないために、
/// SharedKernel 側で「Confirmed」「WaitListed」のみを表す独立した enum を定義する。
/// </remarks>
public enum RegistrationCancelledPriorStatus
{
    /// <summary>キャンセル直前は Confirmed だった。</summary>
    Confirmed = 1,

    /// <summary>キャンセル直前は WaitListed だった。</summary>
    WaitListed = 2,
}

/// <summary>
/// 参加登録がキャンセルされた際に発行されるドメインイベント。
/// </summary>
/// <param name="RegistrationId">登録の ID。</param>
/// <param name="EventId">対象イベントの ID。</param>
/// <param name="PriorStatus">キャンセル直前の登録ステータス（Confirmed / WaitListed）。</param>
/// <param name="OccurredAt">イベント発生日時。</param>
public sealed record RegistrationCancelledEvent(
    Guid RegistrationId,
    Guid EventId,
    RegistrationCancelledPriorStatus PriorStatus,
    DateTimeOffset OccurredAt) : IDomainEvent;
