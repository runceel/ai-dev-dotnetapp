namespace EventRegistration.Analytics.Domain;

/// <summary>
/// 1 日分の集計結果（Read モデル）。日付は UTC で表現する。
/// </summary>
/// <param name="Date">対象日（UTC）。</param>
/// <param name="ConfirmedCount">当日に発生した Confirmed アクティビティ件数。</param>
/// <param name="WaitListedCount">当日に発生した WaitListed アクティビティ件数。</param>
/// <param name="CancelledCount">当日に発生した Cancelled アクティビティ件数。</param>
/// <param name="PromotedCount">当日に発生した PromotedFromWaitList アクティビティ件数。</param>
public sealed record DailyStatistics(
    DateOnly Date,
    int ConfirmedCount,
    int WaitListedCount,
    int CancelledCount,
    int PromotedCount);
