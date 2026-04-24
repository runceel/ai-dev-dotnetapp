using EventRegistration.Events.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventRegistration.Events.Infrastructure;

/// <summary>
/// Events モジュール専用の DbContext。
/// InMemory DB 名 "Events" を使用する。
/// </summary>
public sealed class EventsDbContext : DbContext
{
    public DbSet<Event> Events => Set<Event>();

    public EventsDbContext(DbContextOptions<EventsDbContext> options) : base(options)
    {
    }

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
