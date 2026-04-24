using EventRegistration.Events.Application.Repositories;
using EventRegistration.Events.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventRegistration.Events.Infrastructure.Repositories;

/// <summary>
/// EF Core を使用した <see cref="IEventRepository"/> の実装。
/// </summary>
public sealed class EventRepository : IEventRepository
{
    private readonly EventsDbContext _dbContext;

    public EventRepository(EventsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Events.FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Events.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Event @event, CancellationToken cancellationToken = default)
    {
        _dbContext.Events.Add(@event);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
