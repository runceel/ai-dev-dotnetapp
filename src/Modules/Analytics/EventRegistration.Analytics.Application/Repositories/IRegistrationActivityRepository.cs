using EventRegistration.Analytics.Domain;

namespace EventRegistration.Analytics.Application.Repositories;

/// <summary>
/// Analytics モジュールが保持する <see cref="RegistrationActivity"/> のリポジトリ抽象。
/// 集計結果（Read モデル）の取得もここに集約する。
/// </summary>
public interface IRegistrationActivityRepository
{
    /// <summary>新しいアクティビティを追加する。</summary>
    Task AddAsync(RegistrationActivity activity, CancellationToken cancellationToken = default);

    /// <summary>追加した変更を永続化する。</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 指定イベントの集計結果を取得する。アクティビティが 1 件もなくても、
    /// 全カウント 0 の <see cref="EventStatistics"/> を返す。
    /// </summary>
    Task<EventStatistics> GetEventStatisticsAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 指定イベント・指定日範囲（境界含む）における日別集計を、日付昇順で返す。
    /// </summary>
    Task<IReadOnlyList<DailyStatistics>> GetDailyStatisticsAsync(
        Guid eventId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 集計データが 1 件以上記録されているイベント ID 一覧を返す（重複なし）。
    /// </summary>
    Task<IReadOnlyList<Guid>> GetTrackedEventIdsAsync(CancellationToken cancellationToken = default);
}
