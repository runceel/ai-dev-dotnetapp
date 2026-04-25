namespace EventRegistration.Notifications.Domain;

/// <summary>
/// 通知 1 件分のメッセージ。送信媒体に依存しない最小限のフィールドを保持する。
/// </summary>
/// <param name="Kind">通知の種別。</param>
/// <param name="EventId">関連するイベントの ID。</param>
/// <param name="RegistrationId">関連する参加登録の ID。</param>
/// <param name="ParticipantName">参加者名。</param>
/// <param name="ParticipantEmail">参加者メールアドレス（正規化済）。</param>
public sealed record NotificationMessage(
    NotificationKind Kind,
    Guid EventId,
    Guid RegistrationId,
    string ParticipantName,
    string ParticipantEmail);
