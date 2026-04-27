using EventRegistration.Analytics.Application.Repositories;
using EventRegistration.Analytics.Domain;

namespace EventRegistration.Analytics.Application.UseCases;

/// <summary>
/// 単一イベントの集計結果（<see cref="EventStatistics"/>）を取得するユースケース。
/// </summary>
public sealed class GetEventStatisticsUseCase(IRegistrationActivityRepository repository)
{
    public Task<EventStatistics> ExecuteAsync(Guid eventId, CancellationToken cancellationToken = default)
        => repository.GetEventStatisticsAsync(eventId, cancellationToken);
}
