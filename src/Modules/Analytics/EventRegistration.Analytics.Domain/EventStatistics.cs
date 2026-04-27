namespace EventRegistration.Analytics.Domain;

/// <summary>
/// 個別イベントの集計結果（Read モデル）。
/// </summary>
/// <param name="EventId">対象イベントの ID。</param>
/// <param name="EventName">イベント名。</param>
/// <param name="ScheduledAt">開催予定日時。</param>
/// <param name="Capacity">定員。</param>
/// <param name="ConfirmedCount">参加確定数。</param>
/// <param name="WaitListedCount">キャンセル待ち数。</param>
/// <param name="CancelledCount">キャンセル数。</param>
public sealed record EventStatistics(
    Guid EventId,
    string EventName,
    DateTimeOffset ScheduledAt,
    int Capacity,
    int ConfirmedCount,
    int WaitListedCount,
    int CancelledCount)
{
    /// <summary>
    /// 参加率 ( = 参加確定数 / 定員 )。定員 0 のときは 0。0.0 ～ 1.0 にクランプはしない（超過時は 1.0 を超え得る）。
    /// </summary>
    public double ParticipationRate =>
        Capacity <= 0 ? 0d : (double)ConfirmedCount / Capacity;

    /// <summary>
    /// キャンセル率 ( = キャンセル数 / (キャンセル数 + 確定数 + 待機数) )。総数 0 のときは 0。
    /// </summary>
    public double CancellationRate
    {
        get
        {
            var total = ConfirmedCount + WaitListedCount + CancelledCount;
            return total == 0 ? 0d : (double)CancelledCount / total;
        }
    }
}
