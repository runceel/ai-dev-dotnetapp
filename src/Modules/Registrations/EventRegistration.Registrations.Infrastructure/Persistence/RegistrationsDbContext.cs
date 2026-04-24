using EventRegistration.Registrations.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventRegistration.Registrations.Infrastructure.Persistence;

/// <summary>
/// Registrations モジュールの DbContext。
/// </summary>
public sealed class RegistrationsDbContext(DbContextOptions<RegistrationsDbContext> options) : DbContext(options)
{
    public DbSet<Registration> Registrations => Set<Registration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.EventId).IsRequired();
            entity.Property(r => r.ParticipantName).IsRequired();
            entity.Property(r => r.Email).IsRequired();
            entity.Property(r => r.Status).IsRequired();
            entity.Property(r => r.RegisteredAt).IsRequired();
        });
    }
}
