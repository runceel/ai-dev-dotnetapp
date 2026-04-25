namespace EventRegistration.SharedKernel.Application.Events;

/// <summary>
/// キャンセル待ち（WaitListed）から参加確定（Confirmed）に繰り上がった際に発行されるドメインイベント。
/// </summary>
/// <param name="RegistrationId">繰り上がった登録の ID。</param>
/// <param name="EventId">対象イベントの ID。</param>
/// <param name="ParticipantName">参加者名。</param>
/// <param name="ParticipantEmail">参加者メールアドレス（正規化済）。</param>
/// <param name="OccurredAt">イベント発生日時。</param>
public sealed record ParticipantPromotedFromWaitListEvent(
    Guid RegistrationId,
    Guid EventId,
    string ParticipantName,
    string ParticipantEmail,
    DateTimeOffset OccurredAt) : IDomainEvent;
