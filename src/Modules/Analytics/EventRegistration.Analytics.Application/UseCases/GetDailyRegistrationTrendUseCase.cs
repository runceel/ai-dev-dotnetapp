using EventRegistration.Analytics.Application.Queries;
using EventRegistration.Analytics.Domain;

namespace EventRegistration.Analytics.Application.UseCases;

/// <summary>
/// 日別の登録／キャンセル推移を取得するユースケース。
/// </summary>
public sealed class GetDailyRegistrationTrendUseCase(IAnalyticsQueryService queryService)
{
    /// <summary>
    /// 指定期間 [from, to] の日別推移を取得する。from が to より後の場合は ArgumentException。
    /// </summary>
    public Task<IReadOnlyList<DailyRegistrationPoint>> ExecuteAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        if (from > to)
        {
            throw new ArgumentException("from は to 以前の日付である必要があります。", nameof(from));
        }

        return queryService.GetDailyRegistrationTrendAsync(from, to, cancellationToken);
    }
}
