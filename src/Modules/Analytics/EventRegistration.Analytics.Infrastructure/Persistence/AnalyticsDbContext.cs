using EventRegistration.Analytics.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventRegistration.Analytics.Infrastructure.Persistence;

/// <summary>
/// Analytics モジュールの DbContext。アクティビティ（Read モデルのソース）を保持する。
/// </summary>
public sealed class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : DbContext(options)
{
    public DbSet<RegistrationActivity> Activities => Set<RegistrationActivity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RegistrationActivity>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.EventId).IsRequired();
            entity.Property(a => a.RegistrationId).IsRequired();
            entity.Property(a => a.ActivityType).IsRequired();
            entity.Property(a => a.OccurredAt).IsRequired();

            entity.HasIndex(a => a.EventId);
            entity.HasIndex(a => new { a.EventId, a.OccurredAt });
        });
    }
}
