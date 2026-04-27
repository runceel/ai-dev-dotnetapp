namespace EventRegistration.Analytics.Domain;

/// <summary>
/// Analytics モジュールが追跡する登録アクティビティの種別。
/// </summary>
public enum RegistrationActivityType
{
    /// <summary>新規登録時に Confirmed として確定。</summary>
    Confirmed = 1,

    /// <summary>新規登録時にキャンセル待ち（WaitListed）として確定。</summary>
    WaitListed = 2,

    /// <summary>登録がキャンセルされた。</summary>
    Cancelled = 3,

    /// <summary>キャンセル待ちから繰り上がり Confirmed になった。</summary>
    PromotedFromWaitList = 4,
}
