using EventRegistration.Analytics.Application.Queries;
using EventRegistration.Analytics.Domain;
using EventRegistration.Events.Infrastructure.Persistence;
using EventRegistration.Registrations.Domain;
using EventRegistration.Registrations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventRegistration.Analytics.Infrastructure.Queries;

/// <summary>
/// 既存モジュールの DbContext を Read-only で参照して集計を行う <see cref="IAnalyticsQueryService"/> 実装。
/// </summary>
/// <remarks>
/// 設計判断: Analytics は CQRS の Read 側のみを担うため、他モジュールのドメインモデルや UseCase には依存せず、
/// EF Core の <see cref="EventsDbContext"/> / <see cref="RegistrationsDbContext"/> に対して
/// <c>AsNoTracking</c> で直接クエリを発行する。これは Issue #22 で許容される
/// 「DB 直接参照」パターンに該当する。Analytics 専用の DbContext は持たない（保有データを増やさない）。
/// </remarks>
public sealed class AnalyticsQueryService(
    EventsDbContext eventsDbContext,
    RegistrationsDbContext registrationsDbContext) : IAnalyticsQueryService
{
    public async Task<IReadOnlyList<EventStatistics>> GetEventStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        var events = await eventsDbContext.Events
            .AsNoTracking()
            .OrderByDescending(e => e.ScheduledAt)
            .Select(e => new { e.Id, e.Name, e.ScheduledAt, e.Capacity })
            .ToListAsync(cancellationToken);

        if (events.Count == 0)
        {
            return Array.Empty<EventStatistics>();
        }

        var eventIds = events.Select(e => e.Id).ToList();

        // 各イベント・状態ごとの件数を一括集計
        var counts = await registrationsDbContext.Registrations
            .AsNoTracking()
            .Where(r => eventIds.Contains(r.EventId))
            .GroupBy(r => new { r.EventId, r.Status })
            .Select(g => new { g.Key.EventId, g.Key.Status, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var byEvent = counts
            .GroupBy(c => c.EventId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.Status, x => x.Count));

        var result = new List<EventStatistics>(events.Count);
        foreach (var ev in events)
        {
            byEvent.TryGetValue(ev.Id, out var byStatus);
            int Get(RegistrationStatus s) => byStatus is not null && byStatus.TryGetValue(s, out var c) ? c : 0;

            result.Add(new EventStatistics(
                EventId: ev.Id,
                EventName: ev.Name,
                ScheduledAt: ev.ScheduledAt,
                Capacity: ev.Capacity,
                ConfirmedCount: Get(RegistrationStatus.Confirmed),
                WaitListedCount: Get(RegistrationStatus.WaitListed),
                CancelledCount: Get(RegistrationStatus.Cancelled)));
        }

        return result;
    }

    public async Task<OverallSummary> GetOverallSummaryAsync(CancellationToken cancellationToken = default)
    {
        var totalEvents = await eventsDbContext.Events
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var counts = await registrationsDbContext.Registrations
            .AsNoTracking()
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        int Get(RegistrationStatus s) => counts.FirstOrDefault(c => c.Status == s)?.Count ?? 0;

        var confirmed = Get(RegistrationStatus.Confirmed);
        var waitListed = Get(RegistrationStatus.WaitListed);
        var cancelled = Get(RegistrationStatus.Cancelled);

        return new OverallSummary(
            TotalEvents: totalEvents,
            TotalRegistrations: confirmed + waitListed + cancelled,
            TotalConfirmed: confirmed,
            TotalWaitListed: waitListed,
            TotalCancelled: cancelled);
    }

    public async Task<IReadOnlyList<DailyRegistrationPoint>> GetDailyRegistrationTrendAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var fromUtc = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        // to の翌日 00:00 (UTC) を排他的上限に使用
        var toExclusiveUtc = to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var fromOffset = new DateTimeOffset(fromUtc, TimeSpan.Zero);
        var toOffset = new DateTimeOffset(toExclusiveUtc, TimeSpan.Zero);

        // InMemory プロバイダーでは EF.Functions.* の制限があるため、対象期間内に絞ったうえでメモリ上で集計する。
        var registrations = await registrationsDbContext.Registrations
            .AsNoTracking()
            .Where(r => r.RegisteredAt >= fromOffset && r.RegisteredAt < toOffset)
            .Select(r => r.RegisteredAt)
            .ToListAsync(cancellationToken);

        var cancellations = await registrationsDbContext.Registrations
            .AsNoTracking()
            .Where(r => r.CancelledAt != null
                     && r.CancelledAt >= fromOffset
                     && r.CancelledAt < toOffset)
            .Select(r => r.CancelledAt!.Value)
            .ToListAsync(cancellationToken);

        var registrationsByDate = registrations
            .GroupBy(d => DateOnly.FromDateTime(d.UtcDateTime))
            .ToDictionary(g => g.Key, g => g.Count());

        var cancellationsByDate = cancellations
            .GroupBy(d => DateOnly.FromDateTime(d.UtcDateTime))
            .ToDictionary(g => g.Key, g => g.Count());

        var totalDays = to.DayNumber - from.DayNumber + 1;
        var result = new List<DailyRegistrationPoint>(totalDays);
        for (var day = from; day <= to; day = day.AddDays(1))
        {
            registrationsByDate.TryGetValue(day, out var regCount);
            cancellationsByDate.TryGetValue(day, out var cancelCount);
            result.Add(new DailyRegistrationPoint(day, regCount, cancelCount));
        }

        return result;
    }
}
