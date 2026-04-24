using EventRegistration.Events.Infrastructure.Persistence;
using EventRegistration.Registrations.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace EventRegistration.Web.Adapters;

/// <summary>
/// EventsDbContext を使用して IEventCapacityChecker を実装する反腐敗層アダプター。
/// Registrations モジュールから Events モジュールへの間接アクセスを仲介する。
/// </summary>
public sealed class EventCapacityCheckerAdapter(EventsDbContext eventsDbContext) : IEventCapacityChecker
{
    public async Task<EventCapacityInfo?> GetEventCapacityInfoAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var ev = await eventsDbContext.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        if (ev is null)
        {
            return null;
        }

        return new EventCapacityInfo(ev.Id, ev.Name, ev.Capacity);
    }
}
