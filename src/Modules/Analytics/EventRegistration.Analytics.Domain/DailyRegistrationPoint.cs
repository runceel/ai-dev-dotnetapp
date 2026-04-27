namespace EventRegistration.Analytics.Domain;

/// <summary>
/// 日別の登録推移を表す Read モデル。
/// </summary>
/// <param name="Date">対象日（UTC ベース、時刻は 00:00:00）。</param>
/// <param name="RegistrationCount">その日に新規登録された件数（Confirmed + WaitListed の生成数）。</param>
/// <param name="CancellationCount">その日にキャンセルされた件数。</param>
public sealed record DailyRegistrationPoint(
    DateOnly Date,
    int RegistrationCount,
    int CancellationCount);
