using EventRegistration.Analytics.Domain;

namespace EventRegistration.Analytics.Application.Queries;

/// <summary>
/// Analytics モジュールが提供する Read 専用クエリ。
/// 実装は Infrastructure 層で他モジュールの DbContext を直接読み取る (Read-only 参照)。
/// </summary>
public interface IAnalyticsQueryService
{
    /// <summary>
    /// 全イベントの集計を取得する。
    /// </summary>
    Task<IReadOnlyList<EventStatistics>> GetEventStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// システム全体のサマリーを取得する。
    /// </summary>
    Task<OverallSummary> GetOverallSummaryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 指定期間 [from, to] における日別の登録／キャンセル数推移を取得する。
    /// </summary>
    /// <param name="from">起点（含む、UTC ベースの日付）。</param>
    /// <param name="to">終点（含む、UTC ベースの日付）。</param>
    Task<IReadOnlyList<DailyRegistrationPoint>> GetDailyRegistrationTrendAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);
}
