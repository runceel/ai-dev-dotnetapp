using EventRegistration.Analytics.Application.Repositories;
using EventRegistration.Analytics.Domain;

namespace EventRegistration.Analytics.Application.UseCases;

/// <summary>
/// 指定イベントの日別集計（<see cref="DailyStatistics"/>）を取得するユースケース。
/// </summary>
public sealed class GetDailyStatisticsUseCase(IRegistrationActivityRepository repository)
{
    /// <summary>
    /// 指定イベント・指定日範囲（境界含む）における日別集計を、日付昇順で返す。
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="fromDate"/> が <paramref name="toDate"/> より後の場合。</exception>
    public Task<IReadOnlyList<DailyStatistics>> ExecuteAsync(
        Guid eventId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        if (fromDate > toDate)
        {
            throw new ArgumentException(
                "fromDate は toDate 以前である必要があります。",
                nameof(fromDate));
        }

        return repository.GetDailyStatisticsAsync(eventId, fromDate, toDate, cancellationToken);
    }
}
