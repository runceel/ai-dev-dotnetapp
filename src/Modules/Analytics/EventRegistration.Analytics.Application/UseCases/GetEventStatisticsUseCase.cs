using EventRegistration.Analytics.Application.Queries;
using EventRegistration.Analytics.Domain;

namespace EventRegistration.Analytics.Application.UseCases;

/// <summary>
/// 全イベントの集計を取得するユースケース。
/// </summary>
public sealed class GetEventStatisticsUseCase(IAnalyticsQueryService queryService)
{
    public Task<IReadOnlyList<EventStatistics>> ExecuteAsync(CancellationToken cancellationToken = default) =>
        queryService.GetEventStatisticsAsync(cancellationToken);
}
