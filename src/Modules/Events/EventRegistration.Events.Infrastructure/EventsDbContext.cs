using EventRegistration.Events.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventRegistration.Events.Infrastructure;

/// <summary>
/// Events モジュールの DbContext。InMemory DB を使用する。
/// </summary>
public sealed class EventsDbContext(DbContextOptions<EventsDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.ScheduledAt).IsRequired();
            entity.Property(e => e.Capacity).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}
