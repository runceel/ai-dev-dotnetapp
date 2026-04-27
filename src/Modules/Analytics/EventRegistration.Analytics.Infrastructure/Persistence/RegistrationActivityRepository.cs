using EventRegistration.Analytics.Application.Repositories;
using EventRegistration.Analytics.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventRegistration.Analytics.Infrastructure.Persistence;

/// <summary>
/// <see cref="IRegistrationActivityRepository"/> の EF Core 実装。
/// </summary>
public sealed class RegistrationActivityRepository(AnalyticsDbContext dbContext)
    : IRegistrationActivityRepository
{
    public Task AddAsync(RegistrationActivity activity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(activity);
        return dbContext.Activities.AddAsync(activity, cancellationToken).AsTask();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);

    public async Task<EventStatistics> GetEventStatisticsAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        // 1 度のクエリで全件取得し、メモリ側で集計する（InMemory プロバイダ前提）。
        var activities = await dbContext.Activities
            .AsNoTracking()
            .Where(a => a.EventId == eventId)
            .Select(a => a.ActivityType)
            .ToListAsync(cancellationToken);

        var confirmed = activities.Count(t => t == RegistrationActivityType.Confirmed);
        var waitListed = activities.Count(t => t == RegistrationActivityType.WaitListed);
        var cancelled = activities.Count(t => t == RegistrationActivityType.Cancelled);
        var promoted = activities.Count(t => t == RegistrationActivityType.PromotedFromWaitList);

        return new EventStatistics(eventId, confirmed, waitListed, cancelled, promoted);
    }

    public async Task<IReadOnlyList<DailyStatistics>> GetDailyStatisticsAsync(
        Guid eventId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        if (fromDate > toDate)
        {
            return [];
        }

        var fromUtc = fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtcExclusive = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var rows = await dbContext.Activities
            .AsNoTracking()
            .Where(a => a.EventId == eventId
                        && a.OccurredAt >= fromUtc
                        && a.OccurredAt < toUtcExclusive)
            .Select(a => new { a.ActivityType, a.OccurredAt })
            .ToListAsync(cancellationToken);

        var byDate = rows
            .GroupBy(r => DateOnly.FromDateTime(r.OccurredAt.UtcDateTime))
            .ToDictionary(g => g.Key, g => g.ToList());

        var totalDays = toDate.DayNumber - fromDate.DayNumber + 1;
        var result = new List<DailyStatistics>(totalDays);
        for (var date = fromDate; date <= toDate; date = date.AddDays(1))
        {
            if (byDate.TryGetValue(date, out var entries))
            {
                result.Add(new DailyStatistics(
                    Date: date,
                    ConfirmedCount: entries.Count(e => e.ActivityType == RegistrationActivityType.Confirmed),
                    WaitListedCount: entries.Count(e => e.ActivityType == RegistrationActivityType.WaitListed),
                    CancelledCount: entries.Count(e => e.ActivityType == RegistrationActivityType.Cancelled),
                    PromotedCount: entries.Count(e => e.ActivityType == RegistrationActivityType.PromotedFromWaitList)));
            }
            else
            {
                result.Add(new DailyStatistics(date, 0, 0, 0, 0));
            }
        }

        return result;
    }

    public async Task<IReadOnlyList<Guid>> GetTrackedEventIdsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Activities
            .AsNoTracking()
            .Select(a => a.EventId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
