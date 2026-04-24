using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventRegistration.Events.Infrastructure.Persistence;

/// <summary>
/// IEventRepository の EF Core 実装。
/// </summary>
public sealed class EventRepository(EventsDbContext dbContext) : IEventRepository
{
    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Events.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Events
            .OrderByDescending(e => e.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Event ev, CancellationToken cancellationToken = default)
    {
        await dbContext.Events.AddAsync(ev, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
