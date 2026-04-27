namespace EventRegistration.Analytics.Domain;

/// <summary>
/// 全体サマリー（Read モデル）。
/// </summary>
/// <param name="TotalEvents">登録済みイベントの総数。</param>
/// <param name="TotalRegistrations">全期間・全状態の登録総数（キャンセル含む）。</param>
/// <param name="TotalConfirmed">参加確定の総数。</param>
/// <param name="TotalWaitListed">キャンセル待ちの総数。</param>
/// <param name="TotalCancelled">キャンセル済の総数。</param>
public sealed record OverallSummary(
    int TotalEvents,
    int TotalRegistrations,
    int TotalConfirmed,
    int TotalWaitListed,
    int TotalCancelled)
{
    /// <summary>
    /// 全体キャンセル率 ( = キャンセル数 / 登録総数 )。0 のときは 0。
    /// </summary>
    public double OverallCancellationRate =>
        TotalRegistrations == 0 ? 0d : (double)TotalCancelled / TotalRegistrations;
}
