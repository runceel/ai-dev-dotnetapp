namespace EventRegistration.Analytics.Domain;

/// <summary>
/// 単一イベントの集計結果（Read モデル）。
/// </summary>
/// <param name="EventId">対象イベントの ID。</param>
/// <param name="ConfirmedCount">初期登録時に Confirmed となった件数。</param>
/// <param name="WaitListedCount">初期登録時に WaitListed となった件数（= キャンセル待ち発生回数）。</param>
/// <param name="CancelledCount">キャンセル件数（Confirmed/WaitListed どちらからのキャンセルも含む）。</param>
/// <param name="PromotedCount">キャンセル待ちから繰り上がり Confirmed になった件数。</param>
public sealed record EventStatistics(
    Guid EventId,
    int ConfirmedCount,
    int WaitListedCount,
    int CancelledCount,
    int PromotedCount)
{
    /// <summary>登録総数（初期登録時の Confirmed + WaitListed）。</summary>
    public int TotalRegistrations => ConfirmedCount + WaitListedCount;

    /// <summary>
    /// 最終的な参加確定者数（初期 Confirmed + 繰り上がり - キャンセル、負値は 0 にクランプ）。
    /// </summary>
    public int FinalConfirmedCount =>
        Math.Max(0, ConfirmedCount + PromotedCount - CancelledCount);

    /// <summary>
    /// 最終的な参加率（<see cref="FinalConfirmedCount"/> / <see cref="TotalRegistrations"/>）。
    /// 登録総数が 0 の場合は 0 を返す。
    /// </summary>
    public double ParticipationRate => TotalRegistrations == 0
        ? 0d
        : (double)FinalConfirmedCount / TotalRegistrations;

    /// <summary>
    /// キャンセル率（<see cref="CancelledCount"/> / <see cref="TotalRegistrations"/>）。
    /// 登録総数が 0 の場合は 0 を返す。
    /// </summary>
    public double CancellationRate => TotalRegistrations == 0
        ? 0d
        : (double)CancelledCount / TotalRegistrations;
}
