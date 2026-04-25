namespace EventRegistration.Notifications.Domain;

/// <summary>
/// 通知の種別を表す列挙型。
/// 構造化ログのキー <c>Kind</c> として出力され、購読側でのフィルタリング・ルーティングに利用される。
/// </summary>
public enum NotificationKind
{
    /// <summary>
    /// 参加登録が新規に確定したことを通知する種別。
    /// </summary>
    ParticipantConfirmed,

    /// <summary>
    /// キャンセル待ちから参加確定に繰り上がったことを通知する種別。
    /// </summary>
    ParticipantPromotedFromWaitList,
}
