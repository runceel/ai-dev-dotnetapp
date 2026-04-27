using EventRegistration.Analytics.Application.Queries;
using EventRegistration.Analytics.Domain;

namespace EventRegistration.Analytics.Application.UseCases;

/// <summary>
/// 全体サマリーを取得するユースケース。
/// </summary>
public sealed class GetOverallSummaryUseCase(IAnalyticsQueryService queryService)
{
    public Task<OverallSummary> ExecuteAsync(CancellationToken cancellationToken = default) =>
        queryService.GetOverallSummaryAsync(cancellationToken);
}
