using EventRegistration.Events.Application;
using EventRegistration.Events.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventRegistration.Events.Infrastructure;

/// <summary>
/// <see cref="IEventRepository"/> の EF Core 実装。
/// </summary>
internal sealed class EventRepository(EventsDbContext dbContext) : IEventRepository
{
    public async Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Events
            .OrderByDescending(e => e.ScheduledAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task AddAsync(Event @event, CancellationToken cancellationToken = default)
    {
        await dbContext.Events.AddAsync(@event, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
