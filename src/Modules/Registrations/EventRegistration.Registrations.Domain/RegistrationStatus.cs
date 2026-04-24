namespace EventRegistration.Registrations.Domain;

/// <summary>
/// 登録状態を表す列挙型。
/// </summary>
public enum RegistrationStatus
{
    /// <summary>参加確定（定員以内）。</summary>
    Confirmed,

    /// <summary>キャンセル待ち（定員超過）。</summary>
    WaitListed,

    /// <summary>キャンセル済み。</summary>
    Cancelled,
}
